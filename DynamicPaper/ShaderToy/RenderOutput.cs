namespace Maxstupo.DynamicPaper.ShaderToy {

    using System;
    using System.Collections.Generic;

    public sealed class RenderOutput : IEquatable<RenderOutput> {

        public int BufferId { get; }

        public RenderOutput(int bufferId) {
            this.BufferId = bufferId;
        }

        public override bool Equals(object obj) {
            return Equals(obj as RenderOutput);
        }

        public bool Equals(RenderOutput other) {
            return other != null &&
                   this.BufferId == other.BufferId;
        }

        public override int GetHashCode() {
            return -431692208 + this.BufferId.GetHashCode();
        }

        public static bool operator ==(RenderOutput left, RenderOutput right) {
            return EqualityComparer<RenderOutput>.Default.Equals(left, right);
        }

        public static bool operator !=(RenderOutput left, RenderOutput right) {
            return !(left == right);
        }

    }

}