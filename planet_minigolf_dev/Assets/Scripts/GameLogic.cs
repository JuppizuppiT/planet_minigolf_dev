using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic
{
    public interface ICelestial
    {
        Vector2 Position { get; set; }
        float Radius { get; }
        float Mass { get; }
        bool Collidable { get; }
        float DamagePerSecond { get; }
    }

    public class Planet : ICelestial
    {
        public Planet(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public float Mass => Mathf.Pow(Radius, 1.4f);
        public bool Collidable => true;
        public float DamagePerSecond => 0;
    }

    public class Sun : ICelestial
    {
        public Sun(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public float Mass => Mathf.Pow(Radius, 1.4f);
        public bool Collidable => false;
        public float DamagePerSecond => 100;
    }

    public class Goal : ICelestial
    {
        public Goal(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public float Mass => Mathf.Pow(2 * Radius, 1.4f);
        public bool Collidable => false;
        public float DamagePerSecond => 0;
    }

    public class DeadlyFog : ICelestial
    {
        public DeadlyFog(Vector2 position, float radius, float damagePerSecond)
        {
            Position = position;
            Radius = radius;
            DamagePerSecond = damagePerSecond;
        }

        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public float Mass => 0;
        public bool Collidable => false;
        public float DamagePerSecond { get; set; }
    }

    public const float TickHz = 1000;
    public float GameTick { get; private set; }
    public ICelestial[] Celestials { get; private set; }
    public Vector2 BallStartPosition { get; private set; }
    public Vector2 BallPosition { get; private set; }
    public float BallRotation { get; private set; }
    public float BallRadius { get; private set; }
    public Vector2 BallVelocity { get; private set; }
    public float BallAngularVelocity { get; private set; }
    public ICelestial SnapPlanet { get; private set; }
    public bool Stopped { get; private set; }
    public float BallHealth { get; private set; }
    public enum GameState
    {
        Ongoing,
        GameOverFail,
        GameOverWin
    }
    public GameState State { get; private set; }

    public GameLogic(ICelestial[] celestials, Vector2 ballStartPosition, float ballRadius)
    {
        GameTick = 0;
        Celestials = celestials;
        BallStartPosition = ballStartPosition;
        BallPosition = ballStartPosition;
        BallRotation = 0;
        BallRadius = ballRadius;
        BallVelocity = Vector2.zero;
        BallAngularVelocity = 0;
        SnapPlanet = null;
        Stopped = false;
        BallHealth = 10;
        State = GameState.Ongoing;
    }

    public void TickAdvance(Vector2 shot, bool heal, uint ticks = 1)
    {
        if (ticks > 0)
        {
            Tick(shot, heal);
            ticks--;
        }

        while (ticks > 0)
        {
            Tick(Vector2.zero, false);
            ticks--;
        }
    }

    private void Tick(Vector2 shot, bool healPlanet)
    {
        if (State != GameState.Ongoing)
        {
            return;
        }

        foreach (ICelestial celestial in Celestials)
        {
            float distanceToBall = (BallPosition - celestial.Position).magnitude;
            float surfaceDistance = distanceToBall - (celestial.Radius + BallRadius);
            if (surfaceDistance < 0)
            {
                BallHealth -= celestial.DamagePerSecond / TickHz;

                if (celestial is Goal)
                {
                    State = GameState.GameOverWin;
                    return;
                }
            }
        }

        if (BallHealth <= 0)
        {
            State = GameState.GameOverFail;
            return;
        }

        ProcessPhysicsTick(shot);
    }

    private void ProcessPhysicsTick(Vector2 shot)
    {
        if (shot != Vector2.zero)
        {
            Stopped = false;
        }

        Vector2 acceleration = CalculateAccelerationNew(BallPosition);

        // Find nearest celestial (nearestCelestial should always be SnapPlanet when snapped)
        ICelestial nearestCelestial = null;
        bool isColliding = false;
        {
            float lowestSurfaceDistance = 0;
            foreach (ICelestial celestial in Celestials)
            {
                float distanceToBall = (BallPosition - celestial.Position).magnitude;
                float surfaceDistance = distanceToBall - (celestial.Radius + BallRadius);

                if (surfaceDistance < lowestSurfaceDistance || nearestCelestial == null)
                {
                    nearestCelestial = celestial;
                    lowestSurfaceDistance = surfaceDistance;

                    if (surfaceDistance < 0 && celestial.Collidable)
                    {
                        isColliding = true;
                    }
                }
            }
        }

        // Calculate the radial and tangential components of speed and acceleration
        Vector2 radialSpeed = Vector2.zero;
        Vector2 tangentialSpeed = Vector2.zero;
        Vector2 radialAcceleration = Vector2.zero;
        Vector2 tangentialAcceleration = Vector2.zero;
        if (SnapPlanet != null || isColliding)
        {
            Vector2 planetToBall = BallPosition - nearestCelestial.Position;

            radialSpeed = Vector3.Project(BallVelocity, planetToBall);
            tangentialSpeed = Vector3.Project(BallVelocity, Vector2.Perpendicular(planetToBall));

            radialAcceleration = Vector3.Project(acceleration, planetToBall);
            tangentialAcceleration = Vector3.Project(acceleration, Vector2.Perpendicular(planetToBall).normalized);
        }

        // Handle snapping
        if (SnapPlanet != null || isColliding)
        {
            bool snapCriterion = false;
            Vector2 planetToBall = BallPosition - nearestCelestial.Position;
            float accelerationDirection = Vector2.Dot(radialAcceleration, planetToBall);
            if (accelerationDirection < 0 && radialAcceleration.magnitude > Mathf.Pow(tangentialSpeed.magnitude, 2) / nearestCelestial.Radius)
            {
                snapCriterion = true;
            }

            // Check if the ball should get snapped to a planet
            //Debug.Log(radialSpeed.magnitude);
            if (SnapPlanet == null && snapCriterion && radialSpeed.magnitude < 20)
            {
                SnapPlanet = nearestCelestial;
                BallVelocity = tangentialSpeed;
                radialSpeed = Vector2.zero;
                BallPosition = nearestCelestial.Position + (nearestCelestial.Radius + BallRadius) * (BallPosition - nearestCelestial.Position).normalized;

                Debug.Log("Snapped!");
            }

            // Check if the ball should get unsnapped
            if (SnapPlanet != null && (!snapCriterion || shot != Vector2.zero))
            {
                SnapPlanet = null;
            }
        }

        // Check if the ball should be stopped
        if (SnapPlanet != null)
        {
            if (/*tangentialAcceleration.magnitude < 0.5f && */BallVelocity.magnitude < 0.5f)
            {
                BallVelocity = Vector2.zero;
                Stopped = true;
            }
        }

        if (Stopped)
        {
            return;
        }

        if (SnapPlanet == null)
        {
            BallVelocity += acceleration / TickHz;
            BallVelocity += shot;

            if (isColliding)
            {
                Vector2 planetToBall = BallPosition - nearestCelestial.Position;
                float speedDirection = (Vector2.Dot(BallVelocity, planetToBall) < 0 ? -1 : 1);
                if (speedDirection < 0)
                {
                    radialSpeed = Vector3.Project(BallVelocity, planetToBall);
                    Vector2 tangent = Vector2.Perpendicular(planetToBall);
                    tangentialSpeed = Vector3.Project(BallVelocity, tangent);
                    float tangentialSpeedDirection = (Vector2.Dot(tangentialSpeed, tangent) < 0 ? -1 : 1);

                    // Cross lerp tangentialSpeed <-> BallAngularVelocity
                    float newAngularVelocity = Mathf.Lerp(BallAngularVelocity, tangentialSpeedDirection * tangentialSpeed.magnitude / BallRadius, 0.5f);
                    tangentialSpeed = Vector2.Lerp(tangentialSpeed, BallAngularVelocity * BallRadius * tangent.normalized, 0.3f);
                    BallAngularVelocity = newAngularVelocity;

                    BallVelocity = tangentialSpeed - 0.8f * radialSpeed;
                }
                // Set ball to surface
                BallPosition = nearestCelestial.Position + (nearestCelestial.Radius + BallRadius) * (BallPosition - nearestCelestial.Position).normalized;
            }

            BallPosition += BallVelocity / TickHz;
            BallRotation += BallAngularVelocity / TickHz;

            float drag = 0.0005f;
            BallVelocity -= Mathf.Pow(BallVelocity.magnitude, 2) * drag * BallVelocity.normalized / TickHz;
            float angularDrag = 0.01f;
            BallAngularVelocity -= Mathf.Sign(BallAngularVelocity) * Mathf.Pow(BallAngularVelocity, 2) * angularDrag / TickHz;
        }
        else
        {
            BallVelocity += acceleration / TickHz;

            // Update tangential speed
            Vector2 normal = (BallPosition - SnapPlanet.Position).normalized;
            Vector2 tangent = Vector2.Perpendicular(normal);
            tangentialSpeed = Vector3.Project(BallVelocity, tangent);
            float tangentialSpeedDirection = (Vector2.Dot(tangentialSpeed, tangent) < 0 ? -1 : 1);

            BallAngularVelocity = tangentialSpeedDirection * tangentialSpeed.magnitude / BallRadius;

            BallPosition += tangentialSpeed / TickHz;
            BallRotation += BallAngularVelocity / TickHz;

            // Set ball to surface
            BallPosition = SnapPlanet.Position + (SnapPlanet.Radius + BallRadius) * (BallPosition - SnapPlanet.Position).normalized;

            // Rotate old tangential speed
            normal = (BallPosition - SnapPlanet.Position).normalized;
            tangent = Vector2.Perpendicular(normal);
            tangentialSpeed = tangentialSpeed.magnitude * tangentialSpeedDirection * tangent;

            float drag = 0.01f;
            tangentialSpeed -= Mathf.Pow(tangentialSpeed.magnitude, 2) * drag * tangentialSpeed.normalized / TickHz;
            float linearDrag = 1;
            tangentialSpeed -= tangentialSpeed * linearDrag / TickHz;
            float constantDrag = 1;
            tangentialSpeed -= tangentialSpeed.normalized * constantDrag / TickHz;

            BallVelocity = tangentialSpeed;
        }

        // Wrap BallRotation to range 0 - 2*pi
        BallRotation -= 2 * Mathf.PI * Mathf.Floor(BallRotation / (2 * Mathf.PI));
    }

    public Vector2 CalculateAcceleration(Vector2 position)
    {
        Vector2 acceleration = Vector2.zero;

        float distanceExponent = 1.3f;
        foreach (ICelestial celestial in Celestials)
        {
            Vector2 diff = celestial.Position - position;
            Vector2 direction = diff.normalized;
            float distance = diff.magnitude;
            //float gravity_factor = 9000;

            acceleration += 0.3f * celestial.Mass /* * gravity_factor*/ * direction / Mathf.Pow(distance, distanceExponent);
        }

        return 1000 * acceleration;
    }

    public Vector2 CalculateAccelerationNew(Vector2 position)
    {
        Vector2 totalAcceleration = Vector2.zero;
        Vector2 closestPlanetAccelerationVec = Vector2.zero;

        // Sum up gravitational accelerations of all celestials
        ICelestial nearestCelestial = null;
        float closestPlanetSurfDist = 0;
        foreach (ICelestial celestial in Celestials)
        {
            Vector2 ballPlanetVec = (celestial.Position - position).normalized;

            float distanceToPlanetCenter = (celestial.Position - position).magnitude;

            float gravityDistanceExponent = 1.7f;
            distanceToPlanetCenter = Mathf.Pow(distanceToPlanetCenter, gravityDistanceExponent);
            float gravitationalConstant = 7670;
            float acceleration = (gravitationalConstant * celestial.Mass) / distanceToPlanetCenter;
            Vector2 accelerationVec = ballPlanetVec * acceleration;
            totalAcceleration += accelerationVec;

            // remember gravitational acceleration of closest planet
            float distanceToBall = (BallPosition - celestial.Position).magnitude;
            float surfaceDistance = distanceToBall - (celestial.Radius + BallRadius);

            if (surfaceDistance < closestPlanetSurfDist || nearestCelestial == null)
            {
                nearestCelestial = celestial;
                closestPlanetAccelerationVec = accelerationVec;
                closestPlanetSurfDist = surfaceDistance;
            }
        }

        // Lerp total acceleration and acceleration of closest planet when the ball is near to it
        float planetLocalGravityDistance = 10;
        float planetLocalGravityMinMix = 0;
        if (closestPlanetSurfDist < planetLocalGravityDistance)
        {
            float localGravityRatioBounded = Mathf.Max(planetLocalGravityMinMix, closestPlanetSurfDist / planetLocalGravityDistance);
            totalAcceleration = Vector2.Lerp(closestPlanetAccelerationVec, totalAcceleration, localGravityRatioBounded);
        }

        // Check if/where the ball is in the recenter band
        Vector2 ballToCenterVec = Vector2.zero - position;
        float distanceToCenter = ballToCenterVec.magnitude;
        float recenterGravityMinRadius = 900;
        float recenterGravityMaxRadius = 1400;
        float recenterFactor = Mathf.Clamp01((distanceToCenter - recenterGravityMinRadius) / (recenterGravityMaxRadius - recenterGravityMinRadius));

        // Calculate recenter acceleration
        float recentreGravityMagnitude = 80;
        float recenterFactorEased = Mathf.Sin((recenterFactor - 1) * Mathf.PI / 2) + 1;
        Vector2 recenterAcceleration = ballToCenterVec.normalized * (recenterFactorEased * recentreGravityMagnitude);
        totalAcceleration += recenterAcceleration;

        return 1f * totalAcceleration;
    }
}
