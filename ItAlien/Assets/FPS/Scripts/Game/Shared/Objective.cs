using System;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

namespace Unity.FPS.Game
{
    public abstract class Objective : MonoBehaviour
    {
        [Tooltip("Name of the objective that will be shown on screen")]
        public string Title;

        [Tooltip("Short text explaining the objective that will be shown on screen")]
        public string Description;

        [Tooltip("Whether the objective is required to win or not")]
        public bool IsOptional;

        [Tooltip("Delay before the objective becomes visible")]
        public float DelayVisible;

        [Tooltip("SoundTrack to play during objective")]
        public AudioClip soundTrack;

        public bool IsCompleted { get; private set; }
        public bool IsBlocking() => !(IsOptional || IsCompleted);

        public static event Action<Objective> OnObjectiveCreated;
        public static event Action<Objective> OnObjectiveCompleted;

        AudioSource audioSource;

        protected virtual void Start()
        {
            OnObjectiveCreated?.Invoke(this);

            DisplayMessageEvent displayMessage = Events.DisplayMessageEvent;
            displayMessage.Message = Title;
            displayMessage.DelayBeforeDisplay = 0.0f;
            EventManager.Broadcast(displayMessage);

            if (soundTrack != null)
            {
                GameObject.FindFirstObjectByType<MusicManager>().ChangeTrack(soundTrack);
            }
        }

        public void UpdateObjective(string descriptionText, string counterText, string notificationText)
        {
            ObjectiveUpdateEvent evt = Events.ObjectiveUpdateEvent;
            evt.Objective = this;
            evt.DescriptionText = descriptionText;
            evt.CounterText = counterText;
            evt.NotificationText = notificationText;
            evt.IsComplete = IsCompleted;
            EventManager.Broadcast(evt);
        }

        public void CompleteObjective(string descriptionText, string counterText, string notificationText)
        {
            IsCompleted = true;

            ObjectiveUpdateEvent evt = Events.ObjectiveUpdateEvent;
            evt.Objective = this;
            evt.DescriptionText = descriptionText;
            evt.CounterText = counterText;
            evt.NotificationText = notificationText;
            evt.IsComplete = IsCompleted;
            EventManager.Broadcast(evt);

            OnObjectiveCompleted?.Invoke(this);
            
        }

    }
}