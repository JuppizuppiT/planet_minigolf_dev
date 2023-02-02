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
        int InfectionLevel { get; }
    }

    public class Planet : ICelestial
    {
        public Planet(Vector2 position, float radius, int infectionLevel)
        {
            Position = position;
            Radius = radius;
            InfectionLevel = infectionLevel;
        }

        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public float Mass => Mathf.Pow(Radius, 1.4f);
        public bool Collidable => true;
        public float DamagePerSecond => 0;
        public int InfectionLevel { get; set; }
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
        public int InfectionLevel => 0;
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
        public int InfectionLevel => 0;
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
        public int InfectionLevel => 0;
    }

    public class Pill : ICelestial
    {
        public Pill(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public float Mass => 0;
        public bool Collidable => false;
        public float DamagePerSecond => 0;
        public int InfectionLevel => 0;
    }

    public const float TickHz = 1000;
    public ICelestial[] Celestials { get; private set; }
    public Vector2 BallStartPosition { get; private set; }
    public float BallRadius { get; private set; }

    public enum GameResult
    {
        GameOverFail,
        GameOverWin
    }
    public struct GameState
    {
        public float GameTick;
        public int RemainingMoves;
        public Vector2 BallPosition;
        public float BallRotation;
        public Vector2 BallVelocity;
        public float BallAngularVelocity;
        public ICelestial SnapPlanet;
        public bool Stopped;
        public float BallHealth;
        public GameResult? Result;
        public string GameOverMsg;
        public bool[] CelestialVisible;
        public int LastHealCounter;
    }
    public GameState State;

    public GameLogic(ICelestial[] celestials, Vector2 ballStartPosition, float ballRadius, int remainingMoves)
    {
        State.GameTick = 0;
        State.RemainingMoves = remainingMoves;
        Celestials = celestials;
        BallStartPosition = ballStartPosition;
        State.BallPosition = ballStartPosition;
        State.BallRotation = 0;
        BallRadius = ballRadius;
        State.BallVelocity = Vector2.zero;
        State.BallAngularVelocity = 0;
        State.SnapPlanet = null;
        State.Stopped = false;
        State.BallHealth = 10;
        State.Result = null;
        State.GameOverMsg = "";
        State.CelestialVisible = new bool[celestials.Length];
        for (int i = 0; i < celestials.Length; i++)
        {
            State.CelestialVisible[i] = true;
        }
        State.LastHealCounter = 0;
    }

    public void TickAdvance(Vector2 shot, bool heal, int healCounter, uint ticks = 1)
    {
        if (ticks > 0)
        {
            Tick(shot, heal, healCounter);
            ticks--;
        }

        while (ticks > 0)
        {
            Tick(Vector2.zero, heal, healCounter);
            ticks--;
        }
    }

    private void Tick(Vector2 shot, bool healPlanet, int healCounter)
    {
        if (State.Result != null)
        {
            return;
        }

        State.GameTick++;

        for (int i = 0; i < Celestials.Length; i++)
        {
            float distanceToBall = (State.BallPosition - Celestials[i].Position).magnitude;
            float surfaceDistance = distanceToBall - (Celestials[i].Radius + BallRadius);
            if (surfaceDistance < 0)
            {
                State.BallHealth -= Celestials[i].DamagePerSecond / TickHz;

                if (Celestials[i] is Pill && State.CelestialVisible[i])
                {
                    State.RemainingMoves++;
                    State.CelestialVisible[i] = false;
                }

                if (Celestials[i] is Planet && healPlanet && (Celestials[i] as Planet).InfectionLevel > 0 && healCounter > State.LastHealCounter && shot == Vector2.zero)
                {
                    (Celestials[i] as Planet).InfectionLevel--;
                    State.RemainingMoves--;
                    State.LastHealCounter = healCounter;
                }

                if (Celestials[i] is Goal)
                {
                    foreach (ICelestial celestial in Celestials)
                    {
                        if (celestial.InfectionLevel > 0)
                        {
                            State.Result = GameResult.GameOverFail;
                            State.GameOverMsg = "Not all planets were healed!";
                            return;
                        }
                    }

                    State.Result = GameResult.GameOverWin;
                    return;
                }
            }
        }

        if (State.BallHealth <= 0)
        {
            State.Result = GameResult.GameOverFail;
            State.GameOverMsg = "You died!";
            return;
        }

        if (State.SnapPlanet != null && healPlanet && (State.SnapPlanet as Planet).InfectionLevel > 0 && healCounter > State.LastHealCounter && shot == Vector2.zero)
        {
            (State.SnapPlanet as Planet).InfectionLevel--;
            State.RemainingMoves--;
            State.LastHealCounter = healCounter;
        }

        if (State.RemainingMoves <= 0 && State.Stopped)
        {
            State.Result = GameResult.GameOverFail;
            State.GameOverMsg = "No more moves left!";
            return;
        }

        if (shot != Vector2.zero)
        {
            State.RemainingMoves--;
        }

        ProcessPhysicsTick(shot);
    }

    private void ProcessPhysicsTick(Vector2 shot)
    {
        if (shot != Vector2.zero)
        {
            State.Stopped = false;
        }

        Vector2 acceleration = CalculateAccelerationNew(State.BallPosition);

        // Find nearest celestial (nearestCelestial should always be SnapPlanet when snapped)
        ICelestial nearestCelestial = null;
        bool isColliding = false;
        {
            float lowestSurfaceDistance = 0;
            foreach (ICelestial celestial in Celestials)
            {
                float distanceToBall = (State.BallPosition - celestial.Position).magnitude;
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
        if (State.SnapPlanet != null || isColliding)
        {
            Vector2 planetToBall = State.BallPosition - nearestCelestial.Position;

            radialSpeed = Vector3.Project(State.BallVelocity, planetToBall);
            tangentialSpeed = Vector3.Project(State.BallVelocity, Vector2.Perpendicular(planetToBall));

            radialAcceleration = Vector3.Project(acceleration, planetToBall);
            tangentialAcceleration = Vector3.Project(acceleration, Vector2.Perpendicular(planetToBall).normalized);
        }

        // Handle snapping
        if (State.SnapPlanet != null || isColliding)
        {
            bool snapCriterion = false;
            Vector2 planetToBall = State.BallPosition - nearestCelestial.Position;
            float accelerationDirection = Vector2.Dot(radialAcceleration, planetToBall);
            if (accelerationDirection < 0 && radialAcceleration.magnitude > Mathf.Pow(tangentialSpeed.magnitude, 2) / nearestCelestial.Radius)
            {
                snapCriterion = true;
            }

            // Check if the ball should get snapped to a planet
            //Debug.Log(radialSpeed.magnitude);
            if (State.SnapPlanet == null && snapCriterion && radialSpeed.magnitude < 20)
            {
                State.SnapPlanet = nearestCelestial;
                State.BallVelocity = tangentialSpeed;
                radialSpeed = Vector2.zero;
                State.BallPosition = nearestCelestial.Position + (nearestCelestial.Radius + BallRadius) * (State.BallPosition - nearestCelestial.Position).normalized;

                Debug.Log("Snapped!");
            }

            // Check if the ball should get unsnapped
            if (State.SnapPlanet != null && (!snapCriterion || shot != Vector2.zero))
            {
                State.SnapPlanet = null;
            }
        }

        // Check if the ball should be stopped
        if (State.SnapPlanet != null)
        {
            if (/*tangentialAcceleration.magnitude < 0.5f && */State.BallVelocity.magnitude < 0.5f)
            {
                State.BallVelocity = Vector2.zero;
                State.Stopped = true;
            }
        }

        if (State.Stopped)
        {
            return;
        }

        if (State.SnapPlanet == null)
        {
            State.BallVelocity += acceleration / TickHz;
            State.BallVelocity += shot;

            if (isColliding)
            {
                Vector2 planetToBall = State.BallPosition - nearestCelestial.Position;
                float speedDirection = (Vector2.Dot(State.BallVelocity, planetToBall) < 0 ? -1 : 1);
                if (speedDirection < 0)
                {
                    radialSpeed = Vector3.Project(State.BallVelocity, planetToBall);
                    Vector2 tangent = Vector2.Perpendicular(planetToBall);
                    tangentialSpeed = Vector3.Project(State.BallVelocity, tangent);
                    float tangentialSpeedDirection = (Vector2.Dot(tangentialSpeed, tangent) < 0 ? -1 : 1);

                    // Cross lerp tangentialSpeed <-> BallAngularVelocity
                    float newAngularVelocity = Mathf.Lerp(State.BallAngularVelocity, tangentialSpeedDirection * tangentialSpeed.magnitude / BallRadius, 0.5f);
                    tangentialSpeed = Vector2.Lerp(tangentialSpeed, State.BallAngularVelocity * BallRadius * tangent.normalized, 0.3f);
                    State.BallAngularVelocity = newAngularVelocity;

                    State.BallVelocity = tangentialSpeed - 0.7f * radialSpeed;
                }
                // Set ball to surface
                State.BallPosition = nearestCelestial.Position + (nearestCelestial.Radius + BallRadius) * (State.BallPosition - nearestCelestial.Position).normalized;
            }

            State.BallPosition += State.BallVelocity / TickHz;
            State.BallRotation += State.BallAngularVelocity / TickHz;

            ICelestial goalCelestial = null;
            foreach (ICelestial celestial in Celestials)
            {
                if (celestial is Goal)
                {
                    goalCelestial = celestial;
                }
            }

            float goalSurfDist = (State.BallPosition - goalCelestial.Position).magnitude - BallRadius - goalCelestial.Radius;

            float drag = Mathf.Lerp(0.02f, 0.0005f, Mathf.Clamp01(goalSurfDist / 100));
            State.BallVelocity -= Mathf.Pow(State.BallVelocity.magnitude, 2) * drag * State.BallVelocity.normalized / TickHz;
            float angularDrag = 0.01f;
            State.BallAngularVelocity -= Mathf.Sign(State.BallAngularVelocity) * Mathf.Pow(State.BallAngularVelocity, 2) * angularDrag / TickHz;
        }
        else
        {
            State.BallVelocity += acceleration / TickHz;

            // Update tangential speed
            Vector2 normal = (State.BallPosition - State.SnapPlanet.Position).normalized;
            Vector2 tangent = Vector2.Perpendicular(normal);
            tangentialSpeed = Vector3.Project(State.BallVelocity, tangent);
            float tangentialSpeedDirection = (Vector2.Dot(tangentialSpeed, tangent) < 0 ? -1 : 1);

            State.BallAngularVelocity = tangentialSpeedDirection * tangentialSpeed.magnitude / BallRadius;

            State.BallPosition += tangentialSpeed / TickHz;
            State.BallRotation += State.BallAngularVelocity / TickHz;

            // Set ball to surface
            State.BallPosition = State.SnapPlanet.Position + (State.SnapPlanet.Radius + BallRadius) * (State.BallPosition - State.SnapPlanet.Position).normalized;

            // Rotate old tangential speed
            normal = (State.BallPosition - State.SnapPlanet.Position).normalized;
            tangent = Vector2.Perpendicular(normal);
            tangentialSpeed = tangentialSpeed.magnitude * tangentialSpeedDirection * tangent;

            float drag = 0.01f;
            tangentialSpeed -= Mathf.Pow(tangentialSpeed.magnitude, 2) * drag * tangentialSpeed.normalized / TickHz;
            float linearDrag = 1;
            tangentialSpeed -= tangentialSpeed * linearDrag / TickHz;
            float constantDrag = 1;
            tangentialSpeed -= tangentialSpeed.normalized * constantDrag / TickHz;

            State.BallVelocity = tangentialSpeed;
        }

        // Wrap BallRotation to range 0 - 2*pi
        State.BallRotation -= 2 * Mathf.PI * Mathf.Floor(State.BallRotation / (2 * Mathf.PI));
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

            // Remember gravitational acceleration of closest planet
            float distanceToBall = (State.BallPosition - celestial.Position).magnitude;
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
        Vector2 ballToCenterVec = Vector2.zero - position; // TODO: calculate level center and bounding box
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
