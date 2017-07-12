using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class E_roboDwarfStates : MonoBehaviour {
   

        public State currentState = State.idle;

        public enum State
        {
            current,
            idle,
            walking,
            attacking,
            falling,
            hitreacting,
            dying
    }
    public int fullHP;
    
    public int HitPoints { get; set; }
    public bool IsHitReacting { get; set; }
    public bool IsDying { get; set; }


    private void Start()
    {
        HitPoints = fullHP;
    }


}








