using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine {
	public struct Vector3d {
		public double x;
        public double y;
        public double z;

		public Vector3d(double x, double y, double z) {
			this.x = x;
			this.y = y;
            this.z = z;
		}

        public Vector3d(double x, double y) {
            this.x = x;
            this.y = y;
            this.z = 0;
        }

		public Vector3d(Vector3 v) {
			this.x = v.x;
			this.y = v.y;
            this.z = v.z;
		}


        public Vector3d normalized {
            get {
                Vector3d result = new Vector3d(this.x, this.y, this.z);
                result.Normalize();
                return result;
            }
        }

        public double sqrMagnitude {
            get {
                return this.x * this.x + this.y * this.y + this.z * this.z;
            }
        }

        public double magnitude {
            get {
                return Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
            }
        }


        public static Vector3d zero {
            get {
                return new Vector3d(0.0, 0.0, 0.0);
            }
        }


        public static Vector3d SmoothDamp(Vector3d current, Vector3d target, ref Vector3d currentVelocity, double smoothTime) {
            double deltaTime = Time.deltaTime;
            double maxSpeed = Mathf.Infinity;
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        public static Vector3d SmoothDamp(Vector3d current, Vector3d target, ref Vector3d currentVelocity, double smoothTime, double maxSpeed, double deltaTime) {
            double output_x = 0;
            double output_y = 0;
            double output_z = 0;

            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = Math.Max(0.0001, smoothTime);
            double omega = 2 / smoothTime;

            double x = omega * deltaTime;
            double exp = 1 / (1 + x + 0.48 * x * x + 0.235 * x * x * x);

            double change_x = current.x - target.x;
            double change_y = current.y - target.y;
            double change_z = current.z - target.z;
            Vector3d originalTo = target;

            // Clamp maximum speed
            double maxChange = maxSpeed * smoothTime;

            double maxChangeSq = maxChange * maxChange;
            double sqrmag = change_x * change_x + change_y * change_y + change_z * change_z;
            if (sqrmag > maxChangeSq) {
                var mag = (double) Math.Sqrt(sqrmag);
                change_x = change_x / mag * maxChange;
                change_y = change_y / mag * maxChange;
                change_z = change_z / mag * maxChange;
            }

            target.x = current.x - change_x;
            target.y = current.y - change_y;
            target.z = current.z - change_z;

            double temp_x = (currentVelocity.x + omega * change_x) * deltaTime;
            double temp_y = (currentVelocity.y + omega * change_y) * deltaTime;
            double temp_z = (currentVelocity.z + omega * change_z) * deltaTime;

            currentVelocity.x = (currentVelocity.x - omega * temp_x) * exp;
            currentVelocity.y = (currentVelocity.y - omega * temp_y) * exp;
            currentVelocity.z = (currentVelocity.z - omega * temp_z) * exp;

            output_x = target.x + (change_x + temp_x) * exp;
            output_y = target.y + (change_y + temp_y) * exp;
            output_z = target.z + (change_z + temp_z) * exp;

            // Prevent overshooting
            double origMinusCurrent_x = originalTo.x - current.x;
            double origMinusCurrent_y = originalTo.y - current.y;
            double origMinusCurrent_z = originalTo.z - current.z;
            double outMinusOrig_x = output_x - originalTo.x;
            double outMinusOrig_y = output_y - originalTo.y;
            double outMinusOrig_z = output_z - originalTo.z;

            if (origMinusCurrent_x * outMinusOrig_x + origMinusCurrent_y * outMinusOrig_y + origMinusCurrent_z * outMinusOrig_z > 0) {
                output_x = originalTo.x;
                output_y = originalTo.y;
                output_z = originalTo.z;

                currentVelocity.x = (output_x - originalTo.x) / deltaTime;
                currentVelocity.y = (output_y - originalTo.y) / deltaTime;
                currentVelocity.z = (output_z - originalTo.z) / deltaTime;
            }

            return new Vector3d(output_x, output_y, output_z);
        }



        public static double Distance(Vector3d v1, Vector3d v2) {
            return (v1 - v2).magnitude;
        }

        public void Normalize() {
            double magnitude = this.magnitude;
            if (magnitude > 0.0) {
                this /= magnitude;
                return;
            }
            this = Vector3d.zero;
        }



		public override int GetHashCode() {
            Int64 hash = (Int64) 2166136261;
            hash = (hash * 16777619) ^ this.x.GetHashCode();
            hash = (hash * 16777619) ^ this.y.GetHashCode();
            hash = (hash * 16777619) ^ this.z.GetHashCode();
            return (int) hash;
		}

        public override bool Equals(object other) {
			if (!(other is Vector3d))
				return false;

			Vector3d vector3d = (Vector3d) other;
			return (this.x.Equals(vector3d.x) && this.y.Equals(vector3d.y) && this.z.Equals(vector3d.z));
        }



		public static Vector3d operator +(Vector3d a, Vector3d b) {
			return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
		}

        public static Vector3d operator -(Vector3d a) {
            return new Vector3d(-a.x, -a.y, -a.z);
        }

        public static Vector3d operator -(Vector3d a, Vector3d b) {
            return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3d operator *(double m, Vector3d a) {
            return new Vector3d(a.x  * m, a.y * m, a.z * m);
        }

        public static Vector3d operator *(Vector3d a, double m) {
            return new Vector3d(a.x * m, a.y * m, a.z * m);
        }

        public static Vector3d operator /(Vector3d a, double d) {
            return new Vector3d(a.x / d, a.y / d, a.z / d);
        }

		public static bool operator ==(Vector3d lv, Vector3d rv) {
			return (lv.x == rv.x && lv.y == rv.y && lv.z == rv.z);
		}

        public static bool operator !=(Vector3d lv, Vector3d rv) {
            return (lv.x != rv.x || lv.y != rv.y || lv.z != rv.z);
        }

		public static explicit operator Vector3d(Vector3 v) {
			return new Vector3d((double) v.x, (double) v.y, (double) v.z);
		}

        public static explicit operator Vector3(Vector3d v) {
            return new Vector3((float) v.x, (float) v.y, (float) v.z);
        }
	}
}
