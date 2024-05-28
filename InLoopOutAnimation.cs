using Rage;
using Rage.Native;

namespace SimpleCTRL
{
	// NEEDS WORK

	#region Enums
	/// <summary>
	/// Represents the states of the InLoopOutAnimation.
	/// </summary>
	internal enum State
	{
		Starting,
		Looping,
		Ended
	}
	#endregion

	#region Structs
	/// <summary>
	/// Represents an animation with a dictionary and name.
	/// </summary>
	internal struct Animation
	{
		public string dict;

		public string name;

		public Animation(string animationDictionary, string animationName)
		{
			dict = animationDictionary;
			name = animationName;
		}
	}
    #endregion

    internal class InLoopOutAnimation
    {
        #region Fields
        protected Animation start;
		protected Animation loop;
		protected Animation end;
		protected State state;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the InLoopOutAnimation class.
		/// </summary>
		/// <param name="start">The start animation.</param>
		/// <param name="loop">The loop animation.</param>
		/// <param name="end">The end animation.</param>
		public InLoopOutAnimation(Animation start, Animation loop, Animation end)
		{
			this.start = start;
			this.loop = loop;
			this.end = end;
			state = State.Ended;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Manages the animation state and plays the appropriate animation based on the current state.
		/// </summary>
		/// <param name="ped">The Ped entity to apply the animations to.</param>
		public void Magick(Ped ped)
		{
			if (state == State.Ended)
			{
				PlayStart(ped);
			}
			else if (state == State.Starting && !IsAnimationPlaying(ped, start))
			{
				state = State.Looping;
				PlayLoop(ped);
			}
		}

		/// <summary>
		/// Rewinds the loop animation and stops it, then plays the end animation.
		/// </summary>
		/// <param name="ped">The Ped entity to apply the animations to.</param>
		public void RewindAndStop(Ped ped)
		{
			NativeFunction.CallByHash<bool>(0x28004F88151E03E0, ped, loop.name, loop.dict, true); // STOP_ENTITY_ANIM
			PlayEnd(ped);
		}
		#endregion

		#region Protected Methods
		/// <summary>
		/// Plays the start animation.
		/// </summary>
		/// <param name="ped">The Ped entity to apply the animation to.</param>
		protected void PlayStart(Ped ped)
		{
			// ped.Task.PlayAnimation(start.dict, start.name, 8f, -1, (AnimationFlags)0);
			ped.Tasks.PlayAnimation(start.dict, start.name, -1, 8f, 8f, 0f, AnimationFlags.None);
			state = State.Starting;
		}

		/// <summary>
		/// Plays the loop animation.
		/// </summary>
		/// <param name="ped">The Ped entity to apply the animation to.</param>
		protected void PlayLoop(Ped ped)
		{
			// ped.Task.PlayAnimation(loop.dict, loop.name, 50f, -1, (AnimationFlags)1);
			ped.Tasks.PlayAnimation(loop.dict, loop.name, -1, 8f, 50f, 0f, AnimationFlags.Loop);
			state = State.Looping;
		}

		/// <summary>
		/// Plays the end animation.
		/// </summary>
		/// <param name="ped">The Ped entity to apply the animation to.</param>
		protected void PlayEnd(Ped ped)
		{
			// ped.Task.PlayAnimation(end.dict, end.name, 8f, -1, (AnimationFlags)128);
			ped.Tasks.PlayAnimation(end.dict, end.name, -1, 8f, 8f, 0f, AnimationFlags.Idle);
			state = State.Ended;
		}

		/// <summary>
		/// Checks if a specified animation is currently playing on the given Ped entity.
		/// </summary>
		/// <param name="ped">The Ped entity to check.</param>
		/// <param name="anim">The animation to check.</param>
		/// <returns>True if the animation is playing, false otherwise.</returns>
		protected bool IsAnimationPlaying(Ped ped, Animation anim)
		{
			return NativeFunction.CallByHash<bool>(0x1F0B79228E461EC9, ped, anim.dict, anim.name, 3); // IS_ENTITY_PLAYING_ANIM
		}
        #endregion
    }
}
