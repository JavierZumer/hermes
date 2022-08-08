using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    [CreateAssetMenu(menuName = "GameAction")]
    public class GameAction : ScriptableObject
    {
        public event Action OnGameAction;

        public void InvokeAction()
        {
            OnGameAction?.Invoke();
        }
    }
}

