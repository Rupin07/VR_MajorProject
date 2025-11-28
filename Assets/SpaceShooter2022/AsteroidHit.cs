using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TMPro;

public class AsteroidHit : MonoBehaviour
{
   [SerializeField] private GameObject asteroidExplosion;
   [SerializeField] private GameController gameController;
   [SerializeField] private GameObject popupCanvas;

   private void Awake()
   {
       gameController= FindObjectOfType<GameController>();
   }

   public void AsteroidDestroyed()
   {
       
      Instantiate(asteroidExplosion, transform.position, transform.rotation);

      if (GameController.currentGameStatus == GameController.GameState.Playing)
      {
          //calculate the score for hitting asteroid
          float distanceFromPlayer = Vector3.Distance(transform.position, Vector3.zero);
          // Debug.Log(distanceFromPlayer);
          int bonusPoints = (int)(distanceFromPlayer);
     
          int asteroidScore= 10*bonusPoints;

          //set our text for popup and then instantiate the popup
          popupCanvas.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = asteroidScore.ToString();
          GameObject asteroidPopup= Instantiate(popupCanvas, transform.position, Quaternion.identity);
     
          //adjust scale of popup
          asteroidPopup.transform.localScale = new Vector3(transform.localScale.x * (distanceFromPlayer/10),
              transform.localScale.y * (distanceFromPlayer/10),
              transform.localScale.z * (distanceFromPlayer/10));
         
     
          //pass score to gamecontroller
          gameController.UpdatePlayerScore(asteroidScore);
      }
      
      
     
      Destroy(this.gameObject);
   }
}
