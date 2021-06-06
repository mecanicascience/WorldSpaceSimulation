using System;
using System.Runtime.CompilerServices;

namespace UnityEngine {
    public struct QuaternionD {
        public double x;
        public double y;
        public double z;
        public double w;


        public QuaternionD(double x, double y, double z, double w) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public QuaternionD(Vector3d X, Vector3d Y, Vector3d Z) {
            double num  = X.x;
            double num2 = X.y;
            double num3 = X.z;
            double num4 = Y.x;
            double num5 = Y.y;
            double num6 = Y.z;
            double num7 = Z.x;
            double num8 = Z.y;
            double num9 = Z.z;

            if (num + num5 + num9 >= 0.0) {
                double num10 = num + num5 + num9 + 1.0;
                double num11 = 0.5 / Math.Sqrt(num10);
                this.w = num10 * num11;
                this.z = (num2 - num4) * num11;
                this.y = (num7 - num3) * num11;
                this.x = (num6 - num8) * num11;
                return;
            }

            if (num > num5 && num > num9) {
                double num12 = num - num5 - num9 + 1.0;
                double num13 = 0.5 / Math.Sqrt(num12);
                this.x = num12 * num13;
                this.y = (num2 + num4) * num13;
                this.z = (num7 + num3) * num13;
                this.w = (num6 - num8) * num13;
                return;
            }

            if (num5 > num9) {
                double num14 = -num + num5 - num9 + 1.0;
                double num15 = 0.5 / Math.Sqrt(num14);
                this.y = num14 * num15;
                this.x = (num2 + num4) * num15;
                this.w = (num7 - num3) * num15;
                this.z = (num6 + num8) * num15;
                return;
            }

            double num16 = -num - num5 + num9 + 1.0;
            double num17 = 0.5 / Math.Sqrt(num16);
            this.z = num16 * num17;
            this.w = (num2 - num4) * num17;
            this.x = (num7 + num3) * num17;
            this.y = (num6 + num8) * num17;
        }

        public double this[int index] {
            get {
                switch (index) {
                    case 0:
                        return this.x;
                    case 1:
                        return this.y;
                    case 2:
                        return this.z;
                    case 3:
                        return this.w;
                    default:
                        throw new IndexOutOfRangeException("Invalid QuaternionD index!");
                }
            }
            set {
                switch (index) {
                    case 0:
                        this.x = value;
                        return;
                    case 1:
                        this.y = value;
                        return;
                    case 2:
                        this.z = value;
                        return;
                    case 3:
                        this.w = value;
                        return;
                    default:
                        throw new IndexOutOfRangeException("Invalid QuaternionD index!");
                }
            }
        }
        
        public override int GetHashCode() {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
        }


        public static QuaternionD Euler(double x, double y, double z) {
            return (QuaternionD)Quaternion.Euler((float) x, (float) y, (float) z);
            // return QuaternionD.Internal_FromEulerRad(new Vector3d(x, y, z) * 0.017453292519943295);
        }

        public static QuaternionD Euler(Vector3d euler) {
            return (QuaternionD) Quaternion.Euler((Vector3) euler);
            // return QuaternionD.Internal_FromEulerRad(euler * 0.017453292519943295);
        }


        public override bool Equals(object other) {
            if (!(other is QuaternionD))
                return false;

            QuaternionD quaternionD = (QuaternionD)other;
            return (this.x.Equals(quaternionD.x) && this.y.Equals(quaternionD.y) && this.z.Equals(quaternionD.z) && this.w.Equals(quaternionD.w));
        }

        public static QuaternionD operator *(QuaternionD lhs, QuaternionD rhs) {
            return new QuaternionD(
                lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x,
                lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z
            );
        }

        public static Vector3d operator *(QuaternionD rotation, Vector3d point) {
            double num = rotation.x * 2.0;
            double num2 = rotation.y * 2.0;
            double num3 = rotation.z * 2.0;
            double num4 = rotation.x * num;
            double num5 = rotation.y * num2;
            double num6 = rotation.z * num3;
            double num7 = rotation.x * num2;
            double num8 = rotation.x * num3;
            double num9 = rotation.y * num3;
            double num10 = rotation.w * num;
            double num11 = rotation.w * num2;
            double num12 = rotation.w * num3;

            Vector3d result = new Vector3d(
                (1.0 - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z,
                (num7 + num12) * point.x + (1.0 - (num4 + num6)) * point.y + (num9 - num10) * point.z,
                (num8 - num11) * point.x + (num9 + num10) * point.y + (1.0 - (num4 + num5)) * point.z
            );

            return result;
        }

        public static bool operator ==(QuaternionD lhs, QuaternionD rhs) {
            return (lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w);
        }

        public static bool operator !=(QuaternionD lhs, QuaternionD rhs) {
            return (lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z || lhs.w != rhs.w);
        }

        public static explicit operator QuaternionD(Quaternion v) {
            return new QuaternionD(v.x, v.y, v.z, v.w);
        }
    }
}
