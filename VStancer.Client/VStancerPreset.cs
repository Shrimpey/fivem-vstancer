using System;
using System.Text;

namespace Vstancer.Client
{
    public class VStancerPreset : IEquatable<VStancerPreset>
    {
        public float wheelSizeMinVal;
        public float wheelSizeMaxVal;
        public float wheelWidthMinVal;
        public float wheelWidthMaxVal;

        public static float Precision { get; private set; } = 0.001f;
        public int WheelsCount { get; private set; }
        public int FrontWheelsCount { get; private set; }

        public float[] DefaultRotationY { get; private set; }
        public float[] DefaultOffsetX { get; private set; }
        public float[] RotationY { get; set; }
        public float[] OffsetX { get; set; }
        public float SteeringLock { get; set; }
        public float SuspensionHeight { get; set; }
        public float WheelSize { get; set; }
        public float WheelWidth { get; set; }
        public float DefaultSteeringLock { get; set; }
        public float DefaultSuspensionHeight { get; set; }
        public float DefaultWheelSize { get; set; }
        public float DefaultWheelWidth { get; set; }

        public void SetOffsetFront(float value)
        {
            for (int index = 0; index < FrontWheelsCount; index++)
                OffsetX[index] = (index % 2 == 0) ? value : -value;     
        }

        public void SetOffsetRear(float value)
        {
            for (int index = FrontWheelsCount; index < WheelsCount; index++)
                OffsetX[index] = (index % 2 == 0) ? value : -value;
        }

        public void SetRotationFront(float value)
        {
            for (int index = 0; index < FrontWheelsCount; index++)
                RotationY[index] = (index % 2 == 0) ? value : -value;
        }

        public void SetRotationRear(float value)
        {
            for (int index = FrontWheelsCount; index < WheelsCount; index++)
                RotationY[index] = (index % 2 == 0) ? value : -value;
        }
        
        public void SetSteeringLock(float value) {
            SteeringLock = value;
        }

        public void SetSuspensionHeight(float value) {
            SuspensionHeight = value;
        }

        public void SetWheelSize(float value) {
            WheelSize = value;
        }

        public void SetWheelWidth(float value) {
            WheelWidth = value;
        }

        public bool IsEdited
        {
            get
            {
                for (int index = 0; index < WheelsCount; index++)
                {
                    if ((DefaultOffsetX[index] != OffsetX[index]) || (DefaultRotationY[index] != RotationY[index]))
                        return true;
                }
                if ((DefaultSteeringLock != SteeringLock) || (DefaultSuspensionHeight != SuspensionHeight) || (DefaultWheelSize != WheelSize) || (DefaultWheelWidth != WheelWidth))
                    return true;
                return false;
            }
        }

        public VStancerPreset()
        {
            WheelsCount = 4;
            FrontWheelsCount = 2;

            DefaultRotationY = new float[] { 0, 0, 0, 0 };
            DefaultOffsetX = new float[] { 0, 0, 0, 0 };
            RotationY = new float[] { 0, 0, 0, 0 };
            OffsetX = new float[] { 0, 0, 0, 0 };
            SteeringLock = 65f;
            SuspensionHeight = 0.0f;
            WheelSize = 0.0f;
            WheelWidth = 0.0f;
            DefaultSteeringLock = 65f;
            DefaultSuspensionHeight = 0.0f;
            DefaultWheelSize = 0.0f;
            DefaultWheelWidth = 0.0f;
        }

        public VStancerPreset(int count, float[] defRot, float[] defOff, float defSteerLock, float defSuspHeight, float defWheelSize, float defWheelWidth)
        {
            WheelsCount = count;
            FrontWheelsCount = CalculateFrontWheelsCount(WheelsCount);

            DefaultRotationY = new float[WheelsCount];
            DefaultOffsetX = new float[WheelsCount];
            RotationY = new float[WheelsCount];
            OffsetX = new float[WheelsCount];

            for (int index = 0; index < WheelsCount; index++)
            {
                DefaultRotationY[index] = defRot[index];
                DefaultOffsetX[index] = defOff[index];

                RotationY[index] = DefaultRotationY[index];
                OffsetX[index] = DefaultOffsetX[index];
            }
            DefaultSteeringLock = defSteerLock;
            SteeringLock = DefaultSteeringLock;
            DefaultSuspensionHeight = defSuspHeight;
            SuspensionHeight = DefaultSuspensionHeight;
            DefaultWheelSize = defWheelSize;
            WheelSize = DefaultWheelSize;
            DefaultWheelWidth = defWheelWidth;
            WheelWidth = DefaultWheelWidth;
        }

        public VStancerPreset(int count, float frontOffset, float frontRotation, float rearOffset, float rearRotation, float steeringLock, float suspensionHeight, float wheelSize, float wheelWidth, float defaultFrontOffset, float defaultFrontRotation, float defaultRearOffset, float defaultRearRotation, float defaultSteeringLock, float defaultSuspensionHeight, float defaultWheelSize, float defaultWheelWidth)
        {
            WheelsCount = count;

            DefaultRotationY = new float[WheelsCount];
            DefaultOffsetX = new float[WheelsCount];
            RotationY = new float[WheelsCount];
            OffsetX = new float[WheelsCount];

            FrontWheelsCount = CalculateFrontWheelsCount(WheelsCount);

            for (int index = 0; index < FrontWheelsCount; index++)
            {
                if (index % 2 == 0)
                {
                    DefaultRotationY[index] = defaultFrontRotation;
                    DefaultOffsetX[index] = defaultFrontOffset;
                    RotationY[index] = frontRotation;
                    OffsetX[index] = frontOffset;
                }
                else
                {
                    DefaultRotationY[index] = -defaultFrontRotation;
                    DefaultOffsetX[index] = -defaultFrontOffset;
                    RotationY[index] = -frontRotation;
                    OffsetX[index] = -frontOffset;
                }
            }

            for (int index = FrontWheelsCount; index < WheelsCount; index++)
            {
                if (index % 2 == 0)
                {
                    DefaultRotationY[index] = defaultRearRotation;
                    DefaultOffsetX[index] = defaultRearOffset;
                    RotationY[index] = rearRotation;
                    OffsetX[index] = rearOffset;
                }
                else
                {
                    DefaultRotationY[index] = -defaultRearRotation;
                    DefaultOffsetX[index] = -defaultRearOffset;
                    RotationY[index] = -rearRotation;
                    OffsetX[index] = -rearOffset;
                }
            }
            SteeringLock = steeringLock;
            SuspensionHeight = suspensionHeight;
            DefaultSteeringLock = defaultSteeringLock;
            DefaultSuspensionHeight = defaultSuspensionHeight;
            WheelSize = wheelSize;
            DefaultWheelSize = defaultWheelSize;
            WheelWidth = wheelWidth;
            DefaultWheelWidth = defaultWheelWidth;
        }

        public void Reset()
        {
            for (int index = 0; index < WheelsCount; index++)
            {
                RotationY[index] = DefaultRotationY[index];
                OffsetX[index] = DefaultOffsetX[index];
            }
            SteeringLock = DefaultSteeringLock;
            SuspensionHeight = DefaultSuspensionHeight;
            if(DefaultWheelSize != 0.0f)
                WheelSize = DefaultWheelSize;
            if (DefaultWheelWidth != 0.0f)
                WheelWidth = DefaultWheelWidth;
        }

        public bool Equals(VStancerPreset other)
        {
            if (WheelsCount != other.WheelsCount)
                return false;

            for (int index = 0; index < WheelsCount; index++)
            {
                if (Math.Abs(DefaultOffsetX[index] - other.DefaultOffsetX[index]) > Precision
                    || Math.Abs(DefaultRotationY[index] - other.DefaultRotationY[index]) > Precision
                    || Math.Abs(OffsetX[index] - other.OffsetX[index]) > Precision
                    || Math.Abs(RotationY[index] - other.RotationY[index]) > Precision)
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine($"Edited:{IsEdited} Wheels count:{WheelsCount} Front count:{FrontWheelsCount}");

            StringBuilder defOff = new StringBuilder(string.Format("{0,20}", "Default offset:"));
            StringBuilder defRot = new StringBuilder(string.Format("{0,20}", "Default rotation:"));
            StringBuilder curOff = new StringBuilder(string.Format("{0,20}", "Current offset:"));
            StringBuilder curRot = new StringBuilder(string.Format("{0,20}", "Current rotation:"));

            for (int i = 0; i < WheelsCount; i++)
            {
                defOff.Append(string.Format("{0,15}", DefaultOffsetX[i]));
                defRot.Append(string.Format("{0,15}", DefaultRotationY[i]));
                curOff.Append(string.Format("{0,15}", OffsetX[i]));
                curRot.Append(string.Format("{0,15}", RotationY[i]));
            }

            s.AppendLine(curOff.ToString());
            s.AppendLine(defOff.ToString());
            s.AppendLine(curRot.ToString());
            s.AppendLine(defRot.ToString());

            return s.ToString();
        }


        /// <summary>
        /// Calculate the number of front wheels of a vehicle, starting from the number of all the wheels
        /// </summary>
        /// <param name="wheelsCount">The number of wheels of a such vehicle</param>
        /// <returns></returns>
        public static int CalculateFrontWheelsCount(int wheelsCount)
        {
            int _frontWheelsCount = wheelsCount / 2;

            if (_frontWheelsCount % 2 != 0)
                _frontWheelsCount -= 1;

            return _frontWheelsCount;
        }

        /// <summary>
        /// Returns the preset as an array of floats containing in order: 
        /// frontOffset, frontRotation, rearOffset, rearRotation, steeringLock, suspensionHeight, wheelSize, wheelWidth, defaultFrontOffset, defaultFrontRotation, defaultRearOffset, defaultRearRotation, defaltSteeringLock, defaultSuspensionHeight, defaultWheelSize, defaultWheelWidth
        /// </summary>
        /// <returns>The float array</returns>
        public float[] ToArray()
        {
            return new float[] {
                OffsetX[0],
                RotationY[0],
                OffsetX[FrontWheelsCount],
                RotationY[FrontWheelsCount],
                SteeringLock,
                SuspensionHeight,
                WheelSize,
                WheelWidth,
                DefaultOffsetX[0],
                DefaultRotationY[0],
                DefaultOffsetX[FrontWheelsCount],
                DefaultRotationY[FrontWheelsCount],
                DefaultSteeringLock,
                DefaultSuspensionHeight,
                DefaultWheelSize,
                DefaultWheelWidth,
            };
        }
    }
}
