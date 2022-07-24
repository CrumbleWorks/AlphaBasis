using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        #region Echo Redirecting
        //TODO improve this copied code to run an endless log (scroll)
        public bool resetScreen = true; // Put this at the top of the script

        public void EchoToScreen(string text) // Put this at the top of the script
        {
            if (Me.SurfaceCount > 0)
            {
                string extra = "";
                if (!resetScreen && text.Length != 0) extra = Environment.NewLine;
                IMyTextSurface surface = Me.GetSurface(0);
                surface.ContentType = ContentType.TEXT_AND_IMAGE;
                surface.WriteText(extra + text, !resetScreen);
                resetScreen = false;
            }
        }
        #endregion

        #region State Machine
        private abstract class State
        {
            public virtual void Run(Program p) { }
            public virtual void HandleTransitionFrom(Program p, State previous) { }

            protected void TransitionTo(Program p, State next)
            {
                p.Log($"Transitioning from '{this.GetType().Name}' to '{next.GetType().Name}'", LogLevel.Debug);
                p.activeState = next;
                next.HandleTransitionFrom(p, this);
            }

            public virtual void Start(Program p) { }
            public virtual void Stop(Program p) { }
            public virtual void Reset(Program p) { }

            /// <returns>Unique tag for (de)serialization of the state</returns>
            public abstract string Tag();
            public virtual string Serialize()
            {
                return Tag();
            }

            public static State Deserialize(string data)
            {
                string[] split = data.Split(new[] { ';' }, 2);

                switch (split[0])
                {
                    case "ARC":
                        return new Arc(split[1]);
                    case "CLP":
                        return new Cleanup(split[1]);
                    case "IDL":
                        return new Idle(split[1]);
                    case "EXP":
                        return new Expanding(split[1]);
                    case "HLT":
                        return new Halted(split[1]);
                    case "PLG":
                        return new Plunging();
                    case "RST":
                        return new Resetting(split[1]);
                    case "STP":
                        return new Stopped();
                    case "STR":
                        return new Startup();
                    default:
                        throw new Exception($"Tag: '{split[0]}' cannot be deserialized. No ctor defined!");
                }
            }

            protected void LockDownMachinery(Program p)
            {
                //Rotor
                p.drillArmRotor.RotorLock = true;

                //Arm
                p.drillArmPistons.ForEach(piston => piston.Enabled = false);

                //Drill
                p.drillHead.ForEach(head => head.Enabled = true);

                //Plunger
                p.drillPlungerPistons.ForEach(piston => piston.Enabled = false);
                p.drillPlungerMaglocks.ForEach(maglock => maglock.Lock());
            }

            protected float CalculateRotorVelocityForRadius(Program p, float radius)
            {
                p.Log($"Calculating RPM for radius: {radius}m ...");
                var circumference = 2 * radius * Math.PI;
                var targetRPM = (float)(p.targetSpeedMetersPerSecond / circumference * 60);
                p.Log($"Target RPM: {targetRPM}");
                return targetRPM;
            }
        }

        /// <summary>
        /// Reset the machine back into neutral position
        /// </summary>
        private class Resetting : State
        {
            private readonly float rotorMinAngle;
            private readonly float rotorMaxAngle;

            private bool isRotorReset = false;
            private bool arePistonsReset = false;

            public override string Tag()
            {
                return "RST";
            }

            public override string Serialize()
            {
                return $"{Tag()};{rotorMinAngle};{rotorMaxAngle};{isRotorReset};{arePistonsReset}";
            }

            /// <summary>
            /// Deserialize ctor
            /// </summary>
            internal Resetting(string deserialize)
            {
                string[] split = deserialize.Split(new[] { ';' });
                rotorMinAngle = float.Parse(split[0]);
                rotorMaxAngle = float.Parse(split[1]);
                isRotorReset = bool.Parse(split[2]);
                arePistonsReset = bool.Parse(split[3]);
            }

            public Resetting(Program p)
            {
                if (p.isRotorUpsideDown)
                {
                    throw new Exception("Upside-Down rotors not yet implemented, please install rotor the correct way up!");
                }

                rotorMinAngle = p.drillArmRotor.LowerLimitRad;
                rotorMaxAngle = p.drillArmRotor.UpperLimitRad;
            }

            public override void Run(Program p)
            {
                p.Log("Resetting...");
                p.Log($"Rotating at: {p.drillArmRotor.TargetVelocityRPM}rpm");

                if (!isRotorReset && p.drillArmRotor.Angle == p.armNeutralAngleRads)
                {
                    p.drillArmRotor.RotorLock = true;
                    p.drillArmRotor.TargetVelocityRPM = 0;

                    p.drillArmRotor.LowerLimitRad = rotorMinAngle;
                    p.drillArmRotor.UpperLimitRad = rotorMaxAngle;

                    isRotorReset = true;
                }

                if (!arePistonsReset
                 && p.drillArmPistonsForward.All(piston => piston.MinLimit == piston.CurrentPosition)
                 && p.drillArmPistonsBackward.All(piston => piston.MaxLimit == piston.CurrentPosition)
                 )
                {
                    p.drillArmPistonsForward.ForEach(piston => piston.MaxLimit = piston.MinLimit);
                    p.drillArmPistonsBackward.ForEach(piston => piston.MinLimit = piston.MaxLimit);

                    arePistonsReset = true;
                }

                if (isRotorReset && arePistonsReset)
                {
                    TransitionTo(p, new Stopped());
                }
            }

            public override void HandleTransitionFrom(Program p, State previous)
            {
                LockDownMachinery(p);

                float targetSpeedForRotorRPM = CalculateRotorVelocityForRadius(p, (float)p.armMinRadiusMeters);

                float currentAngle = p.drillArmRotor.Angle;
                if (currentAngle > p.armNeutralAngleRads)
                {
                    p.drillArmRotor.LowerLimitRad = (float)p.armNeutralAngleRads;
                    p.drillArmRotor.TargetVelocityRPM = -targetSpeedForRotorRPM;
                }
                else if (currentAngle < p.armNeutralAngleRads)
                {
                    p.drillArmRotor.UpperLimitRad = (float)p.armNeutralAngleRads;
                    p.drillArmRotor.TargetVelocityRPM = targetSpeedForRotorRPM;
                }

                p.drillArmRotor.RotorLock = false;

                //Pistons
                p.drillArmPistonsForward.ForEach(piston => piston.Retract());
                p.drillArmPistonsBackward.ForEach(piston => piston.Extend());

                p.drillArmPistons.ForEach(piston => piston.Enabled = true);
            }
        }

        /// <summary>
        /// Indicate neutral position
        /// </summary>
        private class Stopped : State
        {
            public override string Tag()
            {
                return "STP";
            }

            public override void Run(Program p)
            {
                p.Log("Stopped...");
            }

            public override void Start(Program p)
            {
                TransitionTo(p, new Startup());
            }

            public override void HandleTransitionFrom(Program p, State previous)
            {
                //TODO turn off standing lights,etc.
            }
        }

        private class Startup : State
        {
            public override string Tag()
            {
                return "STR";
            }

            public override void Run(Program p)
            {
                p.Log("Starting Up...");

                TransitionTo(p, new Arc(p, Arc.Direction.Clockwise));
            }

            public override void HandleTransitionFrom(Program p, State previous)
            {
                //TODO horn alarms, activate lights, etc
            }
        }

        /// <summary>
        /// Indicate that the machine is paused, allows resuming from where it was paused
        /// </summary>
        private class Halted : State
        {
            private State haltedState;

            public override string Tag()
            {
                return "HLT";
            }

            public override string Serialize()
            {
                return $"{Tag()};{haltedState.Tag()};{haltedState.Serialize()}";
            }

            /// <summary>
            /// Deserialize ctor
            /// </summary>
            internal Halted(string deserialize)
            {
                string[] split = deserialize.Split(new[] { ';' }, 2);
                haltedState = Deserialize(split[1]);
            }

            public Halted() { }

            public override void Run(Program p)
            {
                p.Log("Halted...");
                p.Log($"Current state: {haltedState.GetType().Name}");
            }

            public override void HandleTransitionFrom(Program p, State previous)
            {
                haltedState = previous;

                LockDownMachinery(p);

                p.drillHead.ForEach(head => head.Enabled = false);

                //TODO horn alarms, activate info that we're on pause, mood lighting
            }

            public override void Start(Program p)
            {
                TransitionTo(p, haltedState);
            }

            public override void Reset(Program p)
            {
                TransitionTo(p, new Resetting(p));
            }
        }

        /// <summary>
        /// Drill-Arm is moving, lock down rotors
        /// </summary>
        private class Expanding : State
        {
            private readonly Arc.Direction nextDirection;

            public override string Tag()
            {
                return "EXP";
            }

            public override string Serialize()
            {
                return $"{Tag()};{(int)nextDirection}";
            }

            /// <summary>
            /// Deserialize ctor
            /// </summary>
            internal Expanding(string deserialize)
            {
                string[] split = deserialize.Split(new[] { ';' });
                switch (int.Parse(split[0]))
                {
                    case (int)Arc.Direction.Clockwise:
                        nextDirection = Arc.Direction.Clockwise;
                        break;
                    case (int)Arc.Direction.CounterClockwise:
                        nextDirection = Arc.Direction.CounterClockwise;
                        break;
                    default:
                        throw new Exception($"Unknown direction when deserializing Arc state: {split[0]}");
                }
            }

            public Expanding(Program p, Arc.Direction nextDirection)
            {
                this.nextDirection = nextDirection;

                if (p.isRotorUpsideDown)
                {
                    throw new Exception("Upside-Down rotors not yet implemented, please install rotor the correct way up!");
                }

                p.drillArmPistonsForward.ForEach(piston => piston.MaxLimit += p.stepLengthPerPiston);
                p.drillArmPistonsBackward.ForEach(piston => piston.MinLimit -= p.stepLengthPerPiston);
            }

            public override void Run(Program p)
            {
                p.Log("Expanding...");

                if (!(p.drillArmPistonsForward.All(piston => piston.MaxLimit == piston.CurrentPosition)
                  && p.drillArmPistonsBackward.All(piston => piston.MinLimit == piston.CurrentPosition)))
                {
                    return;
                }

                TransitionTo(p, new Arc(p, nextDirection));
            }

            public override void HandleTransitionFrom(Program p, State previous)
            {
                LockDownMachinery(p);

                p.drillArmPistonsForward.ForEach(piston => piston.Extend());
                p.drillArmPistonsBackward.ForEach(piston => piston.Retract());
                p.drillArmPistons.ForEach(piston => piston.Enabled = true);
            }

            public override void Stop(Program p)
            {
                TransitionTo(p, new Halted());
            }

            public override void Reset(Program p)
            {
                TransitionTo(p, new Resetting(p));
            }
        }

        /// <summary>
        /// Drill-Rotor is moving, lock down arms
        /// </summary>
        private class Arc : State
        {
            public enum Direction { Clockwise, CounterClockwise };

            private readonly Direction direction;

            public override string Tag()
            {
                return "ARC";
            }

            public override string Serialize()
            {
                return $"{Tag()};{(int)direction}";
            }

            /// <summary>
            /// Deserialize ctor
            /// </summary>
            internal Arc(string deserialize)
            {
                string[] split = deserialize.Split(new[] { ';' });
                switch (int.Parse(split[0]))
                {
                    case (int)Arc.Direction.Clockwise:
                        direction = Arc.Direction.Clockwise;
                        break;
                    case (int)Direction.CounterClockwise:
                        direction = Arc.Direction.CounterClockwise;
                        break;
                    default:
                        throw new Exception($"Unknown direction when deserializing Arc state: {split[0]}");
                }
            }

            public Arc(Program p, Direction direction)
            {
                if (p.isRotorUpsideDown)
                {
                    throw new Exception("Upside-Down rotors not yet implemented, please install rotor the correct way up!");
                }

                this.direction = direction;

                var radius = p.drillArmPistonsForward.Select(piston => piston.CurrentPosition - piston.MinLimit)
                                                     .Concat(p.drillArmPistonsBackward.Select(piston => piston.MaxLimit - piston.CurrentPosition))
                                                     .Aggregate((float)p.armMinRadiusMeters, (totalLength, length) => totalLength += length);

                float targetSpeedForRotorRPM = CalculateRotorVelocityForRadius(p, radius);
                p.drillArmRotor.TargetVelocityRPM = direction == Direction.Clockwise ? targetSpeedForRotorRPM : -targetSpeedForRotorRPM;
            }

            public override void Run(Program p)
            {
                p.Log("Arcing...");
                p.Log($"Rotating at: {p.drillArmRotor.TargetVelocityRPM}rpm");

                bool v = p.drillArmPistonsForward.All(piston => piston.HighestPosition == piston.CurrentPosition)
                      && p.drillArmPistonsBackward.All(piston => piston.LowestPosition == piston.CurrentPosition);

                switch (direction)
                {
                    case Direction.Clockwise:
                        if (p.drillArmRotor.Angle == p.drillArmRotor.UpperLimitRad)
                        {
                            if (v)
                            {
                                TransitionTo(p, new Cleanup(p, Direction.CounterClockwise));
                            }
                            else
                            {
                                TransitionTo(p, new Idle(new Expanding(p, Direction.CounterClockwise)));
                            }
                        }
                        break;
                    case Direction.CounterClockwise:
                        if (p.drillArmRotor.Angle == p.drillArmRotor.LowerLimitRad)
                        {
                            if (v)
                            {
                                TransitionTo(p, new Cleanup(p, Direction.Clockwise));
                            }
                            else
                            {
                                TransitionTo(p, new Idle(new Expanding(p, Direction.Clockwise)));
                            }
                        }
                        break;
                }
            }

            public override void HandleTransitionFrom(Program p, State previous)
            {
                LockDownMachinery(p);

                p.drillArmRotor.RotorLock = false;
            }

            public override void Stop(Program p)
            {
                TransitionTo(p, new Halted());
            }

            public override void Reset(Program p)
            {
                TransitionTo(p, new Resetting(p));
            }
        }

        /// <summary>
        /// Finishing up contours, everything in use
        /// </summary>
        private class Cleanup : State
        {
            private readonly Arc.Direction direction;
            private readonly float rotorMinAngle;
            private readonly float rotorMaxAngle;

            int currentStep = 0;

            private bool isRotorReset = false;
            private bool arePistonsReset = false;

            public override string Tag()
            {
                return "CLP";
            }

            public override string Serialize()
            {
                return $"{Tag()};{(int)direction};{rotorMinAngle};{rotorMaxAngle};{currentStep};{isRotorReset};{arePistonsReset}";
            }

            /// <summary>
            /// Deserialize ctor
            /// </summary>
            internal Cleanup(string deserialize)
            {
                string[] split = deserialize.Split(new[] { ';' });
                switch (int.Parse(split[0]))
                {
                    case (int)Arc.Direction.Clockwise:
                        direction = Arc.Direction.Clockwise;
                        break;
                    case (int)Arc.Direction.CounterClockwise:
                        direction = Arc.Direction.CounterClockwise;
                        break;
                    default:
                        throw new Exception($"Unknown direction when deserializing Cleanup state: {split[0]}");
                }
                rotorMinAngle = float.Parse(split[1]);
                rotorMaxAngle = float.Parse(split[2]);
                currentStep = int.Parse(split[3]);
                isRotorReset = bool.Parse(split[4]);
                arePistonsReset = bool.Parse(split[5]);
            }

            public Cleanup(Program p, Arc.Direction directionToOtherSide)
            {
                if (p.isRotorUpsideDown)
                {
                    throw new Exception("Upside-Down rotors not yet implemented, please install rotor the correct way up!");
                }

                direction = directionToOtherSide;

                rotorMinAngle = p.drillArmRotor.LowerLimitRad;
                rotorMaxAngle = p.drillArmRotor.UpperLimitRad;
            }

            public override void Run(Program p)
            {
                p.Log("Cleaning Up...");
                p.Log($"Rotating at: {p.drillArmRotor.TargetVelocityRPM}rpm");

                switch (currentStep)
                {
                    case 0: //init
                        LockDownMachinery(p);

                        p.drillArmPistonsForward.ForEach(piston => piston.Retract());
                        p.drillArmPistonsBackward.ForEach(piston => piston.Extend());
                        p.drillArmPistons.ForEach(piston => piston.Enabled = true);

                        ++currentStep;
                        break;
                    case 1: //retracting
                        if (p.drillArmPistonsForward.All(piston => piston.MinLimit == piston.CurrentPosition)
                        && p.drillArmPistonsBackward.All(piston => piston.MaxLimit == piston.CurrentPosition))
                        {
                            LockDownMachinery(p);

                            float targetSpeedForRotorRPM = CalculateRotorVelocityForRadius(p, (float)p.armMinRadiusMeters);
                            p.drillArmRotor.TargetVelocityRPM = direction == Arc.Direction.Clockwise ? targetSpeedForRotorRPM : -targetSpeedForRotorRPM;
                            p.drillArmRotor.RotorLock = false;

                            ++currentStep;
                        }
                        break;
                    case 2: //switching sides
                        switch (direction)
                        {
                            case Arc.Direction.Clockwise:
                                if (p.drillArmRotor.Angle == p.drillArmRotor.UpperLimitRad)
                                {
                                    ++currentStep;
                                }
                                break;
                            case Arc.Direction.CounterClockwise:
                                if (p.drillArmRotor.Angle == p.drillArmRotor.LowerLimitRad)
                                {
                                    ++currentStep;
                                }
                                break;
                        }

                        break;
                    case 3: //starting extension
                        LockDownMachinery(p);

                        p.drillArmPistonsForward.ForEach(piston => piston.Extend());
                        p.drillArmPistonsBackward.ForEach(piston => piston.Retract());
                        p.drillArmPistons.ForEach(piston => piston.Enabled = true);

                        ++currentStep;
                        break;
                    case 4: //extending
                        if (p.drillArmPistonsForward.All(piston => piston.MaxLimit == piston.CurrentPosition)
                        && p.drillArmPistonsBackward.All(piston => piston.MinLimit == piston.CurrentPosition))
                        {
                            LockDownMachinery(p);

                            float targetSpeedForRotorRPM = CalculateRotorVelocityForRadius(p, (float)p.armMinRadiusMeters);

                            float currentAngle = p.drillArmRotor.Angle;
                            if (currentAngle > p.armNeutralAngleRads)
                            {
                                p.drillArmRotor.LowerLimitRad = (float)p.armNeutralAngleRads;
                                p.drillArmRotor.TargetVelocityRPM = -targetSpeedForRotorRPM;
                            }
                            else if (currentAngle < p.armNeutralAngleRads)
                            {
                                p.drillArmRotor.UpperLimitRad = (float)p.armNeutralAngleRads;
                                p.drillArmRotor.TargetVelocityRPM = targetSpeedForRotorRPM;
                            }

                            p.drillArmRotor.RotorLock = false;

                            //Pistons
                            p.drillArmPistonsForward.ForEach(piston => piston.Retract());
                            p.drillArmPistonsBackward.ForEach(piston => piston.Extend());

                            ++currentStep;
                        }

                        break;
                    case 5: //resetting
                        if (!isRotorReset && p.drillArmRotor.Angle == p.armNeutralAngleRads)
                        {
                            LockDownMachinery(p);

                            p.drillArmRotor.RotorLock = true;
                            p.drillArmRotor.TargetVelocityRPM = 0;

                            p.drillArmRotor.LowerLimitRad = rotorMinAngle;
                            p.drillArmRotor.UpperLimitRad = rotorMaxAngle;

                            isRotorReset = true;
                        }

                        if (!arePistonsReset
                         && p.drillArmPistonsForward.All(piston => piston.MinLimit == piston.CurrentPosition)
                         && p.drillArmPistonsBackward.All(piston => piston.MaxLimit == piston.CurrentPosition)
                         )
                        {
                            p.drillArmPistonsForward.ForEach(piston => piston.MaxLimit = piston.MinLimit);
                            p.drillArmPistonsBackward.ForEach(piston => piston.MinLimit = piston.MaxLimit);

                            arePistonsReset = true;
                        }

                        if (isRotorReset && arePistonsReset)
                        {
                            ++currentStep;
                        }
                        break;
                    default:
                        TransitionTo(p, new Plunging(p));
                        break;
                }
            }

            public override void Stop(Program p)
            {
                TransitionTo(p, new Halted());
            }

            public override void Reset(Program p)
            {
                TransitionTo(p, new Resetting(p));
            }
        }

        /// <summary>
        /// Drill-Arm or Drill-Rotor has finished moving, lock arm and rotors.
        /// Stay idle for a few seconds to properly clean up and produce smooth edges before entering the next state.
        /// </summary>
        private class Idle : State
        {
            private readonly State next;
            private int idlingCount = 0;

            public override string Tag()
            {
                return "IDL";
            }

            public override string Serialize()
            {
                return $"{Tag()};{idlingCount};{next.Tag()};{next.Serialize()}";
            }

            /// <summary>
            /// Deserialize ctor
            /// </summary>
            internal Idle(string deserialize)
            {
                string[] split = deserialize.Split(new[] { ';' }, 2);
                idlingCount = int.Parse(split[0]);
                next = Deserialize(split[1]);
            }

            public Idle(State next)
            {
                this.next = next;
            }

            public override void Run(Program p)
            {
                p.Log("Idling...");
                p.Log($"{p.idlingRunLimit - idlingCount}");

                idlingCount++;
                if (idlingCount >= p.idlingRunLimit)
                {
                    TransitionTo(p, next);
                }
            }

            public override void HandleTransitionFrom(Program p, State previous)
            {
                LockDownMachinery(p);
            }

            public override void Stop(Program p)
            {
                TransitionTo(p, new Halted());
            }

            public override void Reset(Program p)
            {
                TransitionTo(p, new Resetting(p));
            }
        }

        /// <summary>
        /// Drill is getting lowered, lock arm and rotors
        /// </summary>
        private class Plunging : State
        {
            public override string Tag()
            {
                return "PLG";
            }

            /// <summary>
            /// Deserialize ctor
            /// </summary>
            internal Plunging() { }

            public Plunging(Program p)
            {
                p.drillPlungerPistons.ForEach(piston => piston.MaxLimit += p.plungeDepthPerPiston);
            }

            public override void Run(Program p)
            {
                p.Log("Plunging...");

                if (!p.drillPlungerPistons.All(piston => piston.CurrentPosition == piston.MaxLimit))
                {
                    return;
                }

                TransitionTo(p, new Startup());
            }

            public override void HandleTransitionFrom(Program p, State previous)
            {
                LockDownMachinery(p);

                p.drillPlungerPistons.ForEach(piston => piston.Enabled = true);
                p.drillPlungerMaglocks.ForEach(maglock => maglock.Unlock());
            }

            public override void Stop(Program p)
            {
                TransitionTo(p, new Halted());
            }

            public override void Reset(Program p)
            {
                TransitionTo(p, new Resetting(p));
            }
        }
        #endregion

        #region Configuration
        //Drill
        private const string IniSectionDrill = @"Drill";

        private const string IniKeyGroupDrillArm = @"DrillArm";
        private const string TagDrillArmPistonBackward = @"[PB]";
        private const string TagDrillArmPistonForward = @"[PF]";
        private const string TagDrillArmRotator = @"[PR]";

        private const string IniKeyGroupDrillHead = @"DrillHead";

        private const string IniKeyGroupDrillPlunger = @"DrillPlunger";
        private const string TagDrillPlungerPiston = @"[PDP]";
        private const string TagDrillPlungerMaglock = @"[PDM]";

        private const string IniKeyGroupDrillBuffer = @"DrillBuffer";
        private const string IniKeyDrillBufferMaxFill = @"DrillBuffer_MaxPercent";
        private const int DrillBufferMaxFillPercentDefault = 80;

        private const string IniKeyTargetSpeed = @"TargetSpeed_MetersSecond";
        private const double TargetSpeedDefault = 0.25;
        private const string IniKeyStepLength = @"StepLength_Meters";
        private const double StepLengthDefault = 2.5;
        private const string IniKeyPlungeDepth = @"PlungeDepth_Meters";
        private const double PlungeDepthDefault = 2.5;

        private const string IniKeyMinArmRadius = @"DrillArm_StaticRadius_Meters";
        private const double MinArmRadiusDefault = 6.25; //half a rotor and a pistonbase

        private const string IniKeyDrillArmRotorNeutralAngleDegrees = @"RotorNeutralPosition_Degrees";
        private const double ArmRotorNeutralAngleDegreesDefault = 0.0;
        private const string IniKeyDrillArmRotorUpsideDown = @"IsRotorUpsideDown";
        private const bool ArmRotorUpsideDownDefault = false;

        private const string IniKeyIdlingTime = @"IdlingTime_Seconds";
        private const int IdlingTimeSecondsDefault = 5;

        //Extender System
        private const string IniSectionExtensionSystem = @"ExtensionSystem";

        private const string IniKeyGroupDrillTranslator = @"DrillTranslator";
        private const string TagDrillTranslatorPiston = @"[PAP]";
        private const string TagDrillTranslatorMaglock = @"[PAM]";
        #endregion

        #region Arguments
        private const string ArgumentStart = @"run";
        private const string ArgumentPause = @"halt";
        private const string ArgumentReset = @"reset";
        #endregion

        private const int TicksPerSecond = 60; //According to google, no idea how true :/

        private const string OreType = @"MyObjectBuilder_Ore";
        private const string RotorType = @"Stator";

        private readonly MyIni _ini;

        #region Config Values
        private bool isRunningDrill = false;
        private bool isRunningExtensionSystem = false;

        private List<IMyFunctionalBlock> drillArm;
        private List<IMyPistonBase> drillArmPistons;
        private List<IMyPistonBase> drillArmPistonsForward;
        private List<IMyPistonBase> drillArmPistonsBackward;
        private IMyMotorStator drillArmRotor;

        private List<IMyFunctionalBlock> drillHead;

        private List<IMyFunctionalBlock> drillPlunger;
        private List<IMyPistonBase> drillPlungerPistons;
        private List<IMyLandingGear> drillPlungerMaglocks;

        private List<IMyInventory> drillBufferInventories;
        private int drillBufferMaxFillPercent;

        private double targetSpeedMetersPerSecond;
        private double stepLenghtMeters;
        private double plungeDepthMeters;

        private double armMinRadiusMeters;

        private double armNeutralAngleRads;
        private bool isRotorUpsideDown;

        private int idlingTimeSeconds;
        #endregion

        #region Calculated Values
        private readonly float stepLengthPerPiston;
        private readonly float maxArmLength;
        private readonly float plungeDepthPerPiston;
        private readonly float maxPlungeDepth;

        private readonly int idlingRunLimit;

        private readonly float drillBufferMaxFill;
        #endregion

        private State activeState;

        public Program()
        {
            _ini = new MyIni();
            SetupLogger();
            ParseConfiguration();

            Log("Calculating Derivative Values");
            //derivative values
            stepLengthPerPiston = (float)(stepLenghtMeters / drillArmPistons.Count());
            plungeDepthPerPiston = (float)(plungeDepthMeters / drillPlungerPistons.Count());

            maxArmLength = drillArmPistonsForward.Select(piston => piston.HighestPosition - piston.MinLimit)
                                                 .Concat(drillArmPistonsBackward.Select(piston => piston.MaxLimit - piston.LowestPosition))
                                                 .Aggregate(0.0f, (totalLength, length) => totalLength += length);
            maxPlungeDepth = drillPlungerPistons.Select(piston => piston.HighestPosition - piston.MinLimit)
                                                .Aggregate(0.0f, (totalLength, length) => totalLength += length);

            Log("Setting Update Frequency and Idling Time");
            Runtime.UpdateFrequency = UpdateFrequency.Update100; //always adjust this together with the value below!
            idlingRunLimit = idlingTimeSeconds * TicksPerSecond / 100;

            Log($"Calculating Buffer Fill Tresholds for {drillBufferMaxFillPercent }%");
            drillBufferMaxFill = drillBufferInventories.Select(inventory => ((float)inventory.MaxVolume)).Aggregate(0.0f, (maxVolume, volume) => maxVolume += volume) * drillBufferMaxFillPercent / 100;

            //load storage
            if (Storage.Length > 0)
            {
                activeState = State.Deserialize(Storage);
                Log(@"Loaded state from storage. Continuing from last save.", LogLevel.Info);
            }

            activeState = new Stopped();
            Echo = EchoToScreen; // Echo Redirecting
        }

        public void Save()
        {
            Storage = activeState.Serialize();
            Log(@"Saved state to storage.", LogLevel.Info);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            resetScreen = true; // Echo Redirecting, Reset Log

            //regular update
            if ((updateSource & (UpdateType.Update1 | UpdateType.Update10 | UpdateType.Update100)) != 0)
            {
                if (CheckIfBuffersHaveSpaceLeft())
                {
                    activeState.Run(this);
                }

                return;
            }

            //update due to anything else than regular ticking
            if (argument.Length < 0)
            {
                return;
            }

            Log($"Processing script argument: '{argument}'");
            switch (argument.ToLower())
            {
                case ArgumentStart:
                    activeState.Start(this);
                    break;
                case ArgumentPause:
                    activeState.Stop(this);
                    break;
                case ArgumentReset:
                    activeState.Reset(this);
                    break;
                default:
                    Log($"Unknown argument: '{argument}'. Ignoring input.", LogLevel.Error);
                    break;
            }
        }

        private bool CheckIfBuffersHaveSpaceLeft()
        {
            var drillBufferCurrent = drillBufferInventories.Select(inventory => ((float)inventory.CurrentVolume)).Aggregate(0.0f, (currVolume, volume) => currVolume += volume);
            Log($"Buffers at {drillBufferCurrent / drillBufferMaxFill * 100}%");

            if (drillBufferCurrent >= drillBufferMaxFill)
            {
                Log($"Buffers full! Waiting for buffers to empty.");

                activeState.Stop(this);
                return false;
            }

            return true;
        }

        #region Configuration
        private bool hasErrors = false;
        private void ParseConfiguration()
        {
            Log("Parsing Configuration");

            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                throw new Exception(result.ToString());
            }

            #region Drill
            //Configs for the operation of a plunger drill
            if (_ini.ContainsSection(IniSectionDrill))
            {
                Log("Parsing Drill Section");

                isRunningDrill = true;

                string value;

                if (TryGetStringValue(IniSectionDrill, IniKeyGroupDrillArm, out value))
                {
                    Log("Parsing Drill-Arm Group");

                    drillArm = new List<IMyFunctionalBlock>();
                    GridTerminalSystem.GetBlockGroupWithName(value).GetBlocksOfType(drillArm, block => block is IMyPistonBase || block is IMyMotorStator);
                    CheckListNotEmpty(drillArm, value, IniSectionDrill, IniKeyGroupDrillArm, @"Piston", @"Rotor");
                    CheckListHasAtMostOneOf(drillArm, value, IniSectionDrill, IniKeyGroupDrillArm, RotorType, @"Rotor");

                    drillArmPistonsForward = drillArm.Where(block => block.CustomName.Contains(TagDrillArmPistonForward)).Select(block => (IMyPistonBase)block).ToList();
                    drillArmPistonsBackward = drillArm.Where(block => block.CustomName.Contains(TagDrillArmPistonBackward)).Select(block => (IMyPistonBase)block).ToList();
                    drillArmPistons = new List<IMyPistonBase>().Concat(drillArmPistonsForward).Concat(drillArmPistonsBackward).ToList();

                    drillArmRotor = (IMyMotorStator)drillArm.First(block => block.CustomName.Contains(TagDrillArmRotator));
                }

                if (TryGetStringValue(IniSectionDrill, IniKeyGroupDrillHead, out value))
                {
                    Log("Parsing Drill-Head Group");

                    drillHead = new List<IMyFunctionalBlock>();
                    GridTerminalSystem.GetBlockGroupWithName(value).GetBlocksOfType(drillHead);
                    CheckListNotEmpty(drillHead, value, IniSectionDrill, IniKeyGroupDrillHead, @"Drill");
                }

                if (TryGetStringValue(IniSectionDrill, IniKeyGroupDrillPlunger, out value))
                {
                    Log("Parsing Drill-Plunger Group");

                    drillPlunger = new List<IMyFunctionalBlock>();
                    GridTerminalSystem.GetBlockGroupWithName(value).GetBlocksOfType(drillPlunger, block => block is IMyPistonBase || block is IMyLandingGear);
                    CheckListNotEmpty(drillPlunger, value, IniSectionDrill, IniKeyGroupDrillPlunger, @"Piston");

                    drillPlungerPistons = drillArm.Where(block => block.CustomName.Contains(TagDrillPlungerPiston)).Select(block => (IMyPistonBase)block).ToList();
                    drillPlungerMaglocks = drillArm.Where(block => block.CustomName.Contains(TagDrillPlungerMaglock)).Select(block => (IMyLandingGear)block).ToList();
                }

                if (TryGetStringValue(IniSectionDrill, IniKeyGroupDrillBuffer, out value))
                {
                    Log("Parsing Drill-Buffer Group");

                    var drillBufferContainers = new List<IMyInventoryOwner>();
                    GridTerminalSystem.GetBlockGroupWithName(value).GetBlocksOfType(drillBufferContainers);

                    drillBufferInventories = drillBufferContainers.SelectMany(block =>
                    {
                        var inventories = new List<IMyInventory>();
                        for (int i = 0; i < block.InventoryCount; i++)
                        {
                            var inventory = block.GetInventory(i);
                            var acceptedItems = new List<MyItemType>();
                            inventory.GetAcceptedItems(acceptedItems, item => item.TypeId.Equals(OreType));
                            if (acceptedItems.Any())
                            {
                                inventories.Add(inventory);
                            }
                        }

                        return inventories;
                    }).ToList();

                    CheckListNotEmpty(drillBufferInventories, value, IniSectionDrill, IniKeyGroupDrillBuffer, @"Container capable of storing ores");
                }
                drillBufferMaxFillPercent = GetIntValueOrDefault(IniSectionDrill, IniKeyDrillBufferMaxFill, DrillBufferMaxFillPercentDefault);

                targetSpeedMetersPerSecond = GetDoubleValueOrDefault(IniSectionDrill, IniKeyTargetSpeed, TargetSpeedDefault);
                stepLenghtMeters = GetDoubleValueOrDefault(IniSectionDrill, IniKeyStepLength, StepLengthDefault);
                plungeDepthMeters = GetDoubleValueOrDefault(IniSectionDrill, IniKeyPlungeDepth, PlungeDepthDefault);

                armNeutralAngleRads = DegreeToRadian(GetDoubleValueOrDefault(IniSectionDrill, IniKeyDrillArmRotorNeutralAngleDegrees, ArmRotorNeutralAngleDegreesDefault));

                idlingTimeSeconds = GetIntValueOrDefault(IniSectionDrill, IniKeyIdlingTime, IdlingTimeSecondsDefault);

                isRotorUpsideDown = GetBoolValueOrDefault(IniSectionDrill, IniKeyDrillArmRotorUpsideDown, ArmRotorUpsideDownDefault);

                armMinRadiusMeters = GetDoubleValueOrDefault(IniSectionDrill, IniKeyMinArmRadius, MinArmRadiusDefault);
            }

            #endregion

            #region Extension System
            //Configs to operate the automatic extension system, allowing to 'endlessly' run the drill
            if (_ini.ContainsSection(IniSectionExtensionSystem))
            {
                Log("Parsing Extender Section");

                isRunningExtensionSystem = true;
                Log(@"Extension System not yet implemented!", LogLevel.Warning);
            }

            #endregion

            if (!(isRunningDrill || isRunningExtensionSystem))
            {
                Me.Enabled = false;
                throw new Exception("No functions configured, script does not do anything. Shutting down.");
            }

            if (hasErrors)
            {
                Me.Enabled = false;
                throw new Exception("Configuration has errors. Shutting down.");
            }
        }

        private void CheckListNotEmpty<T>(List<T> list, string group, string section, string key, params string[] requiredBlocks)
        {
            if (list.Any())
            {
                return;
            }

            hasErrors = true;
            Log($"The group '{group}' defined for '{key}' in '[{section}]' does not contain any valid blocks. Make sure it contains at least one {string.Join(" and one ", requiredBlocks)}.", LogLevel.Error);
        }

        private void CheckListHasAtMostOneOf<T>(List<T> list, string group, string section, string key, string restrictedBlockType, string restrictedBlockName)
        {
            int amountOfRestrictedBlocks = list.Where(block => ((IMyCubeBlock)block).BlockDefinition.TypeIdString.Contains(restrictedBlockType)).Count();
            if (amountOfRestrictedBlocks > 0 && amountOfRestrictedBlocks <= 1)
            {
                return;
            }

            hasErrors = true;
            Log($"The group '{group}' defined for '{key}' in '[{section}]' may contain at most one {restrictedBlockName}. Instead contains: {amountOfRestrictedBlocks}.", LogLevel.Error);
        }

        private bool TryGetStringValue(string section, string key, out string valueString)
        {
            MyIniValue valueIni = _ini.Get(section, key);
            valueString = null;
            if (valueIni.IsEmpty || !valueIni.TryGetString(out valueString) || string.IsNullOrWhiteSpace(valueString))
            {
                hasErrors = true;
                Log($"'{key}' in '[{section}]' is either undefined or empty. Please define it like '{key}=<MyValue>'.", LogLevel.Error);
                return false;
            }

            return true;
        }

        private double GetDoubleValueOrDefault(string section, string key, double defaultValue)
        {
            MyIniValue valueIni = _ini.Get(section, key);
            if (valueIni.IsEmpty)
            {
                return defaultValue;
            }

            return valueIni.ToDouble(defaultValue);
        }

        private int GetIntValueOrDefault(string section, string key, int defaultValue)
        {
            MyIniValue valueIni = _ini.Get(section, key);
            if (valueIni.IsEmpty)
            {
                return defaultValue;
            }

            return valueIni.ToInt32(defaultValue);
        }

        private bool GetBoolValueOrDefault(string section, string key, bool defaultValue)
        {
            MyIniValue valueIni = _ini.Get(section, key);
            if (valueIni.IsEmpty)
            {
                return defaultValue;
            }

            return valueIni.ToBoolean(defaultValue);
        }
        #endregion

        #region Logger
        private const string IniSectionLogger = @"Logger";
        private const string IniKeyLogLevel = @"LogLevel";
        enum LogLevel { Trace, Debug, Info, Warning, Error }
        private LogLevel configuredLogLevel = LogLevel.Info;
        private void SetupLogger()
        {
            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                throw new Exception(result.ToString());
            }

            if (!_ini.ContainsSection(IniSectionLogger))
            {
                return;
            }

            if (!_ini.ContainsKey(IniSectionLogger, IniKeyLogLevel))
            {
                Log(@"No custom LogLevel defined. Define one of: Trace, Debug, Info, Warning, Error", LogLevel.Warning);
                return;
            }

            var customLevelIni = _ini.Get(IniSectionLogger, IniKeyLogLevel);
            LogLevel customLevel;
            var customLevelString = customLevelIni.ToString();
            if (!Enum.TryParse(customLevelString, out customLevel))
            {
                Log($"Custom LogLevel '{customLevelString}' unknown. Define one of: Trace, Debug, Info, Warning, Error", LogLevel.Warning);
                return;
            }

            configuredLogLevel = customLevel;
            Log($"Set LogLevel to '{customLevel}'");
        }

        private void Log(string m, LogLevel l = LogLevel.Debug)
        {
            if (l < configuredLogLevel)
            {
                return;
            }

            switch (l)
            {
                case LogLevel.Trace:
                    Echo($"TRC: {m}");
                    break;
                default:
                case LogLevel.Debug:
                    Echo($"DBG: {m}");
                    break;
                case LogLevel.Info:
                    Echo($"INF: {m}");
                    break;
                case LogLevel.Warning:
                    Echo($"WRN: {m}");
                    break;
                case LogLevel.Error:
                    Echo($"ERR: {m}");
                    break;
            }
        }
        #endregion

        #region Helpers
        private static double RadianToDegree(double radians)
        {
            return radians * (180.0f / Math.PI);
        }

        private static double DegreeToRadian(double degrees)
        {
            return degrees * (Math.PI / 180.0f);
        }
        #endregion
    }
}
