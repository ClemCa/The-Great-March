using UnityEngine;
using System;


public class clickToChangeFlare:MonoBehaviour{
    public Flare flare1;
    public Flare flare2;
    public Light lig;
    public void Update() {
        if(InputHelper.MouseLeftDown)
        {
        if(lig.flare == flare1){
         lig.flare = flare2;
        }else{
         lig.flare = flare1;
        }
       
        
        }
        }
}