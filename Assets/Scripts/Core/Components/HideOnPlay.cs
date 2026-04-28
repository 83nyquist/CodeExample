using UnityEngine;

namespace Core.Components
{
	//Remember to set execution order for this to last in Edit | Project Settings | Script Execution Order
	
	public class HideOnPlay : MonoBehaviour
	{    
		void Start ()
		{
			gameObject.SetActive(false);
		}
	}
}
