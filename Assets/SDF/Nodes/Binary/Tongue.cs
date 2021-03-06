﻿using System;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class Tongue : SDFNodeBinary<Tongue.Op> {

        public float RadiusA { get; set; }
        public float RadiusB { get; set; }

        public override bool IsCommutative => false;

        protected override Op GetOp() {
            return new Op() {
                RadiusA = RadiusA,
                RadiusB = RadiusB
            };
        }

        public struct Op : IBinaryOp {
            public float RadiusA;
            public float RadiusB;

            public float Combine(float left, float right) {
                return Tongue.Combine(left, right, RadiusA, RadiusB);
            }
        }

        public static float Combine(float left, float right, float radiusA, float radiusB) {
            return min(left, max(left - radiusA, abs(right) - radiusB));
        }
    }
}
