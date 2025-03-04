using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class MoleController : MonoBehaviour
{
    public float popUpSpeed = 5f; //mole pop up
    public float hideSpeed = 5f; //mole back
    public float stayDuration = 1f; //stay time

    private Vector3 hiddenPosition; //mole original hidden position
    private Vector3 targetPosition; //mole out position
    private bool isActive = false;
    
    // Start is called before the first frame update
    void Start()
    {
        hiddenPosition = transform.position + Vector3.down * 0.5f; // Invisible position
        targetPosition = transform.position + Vector3.up * 0.1f; // pop up; (visible position)
        Hide(); // hide the mole at the beginning
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PopUp()
    {
        if (isActive)
        {
            return;
        }
        isActive = true;
        StopAllCoroutines();
        StartCoroutine(MoveTo(targetPosition, popUpSpeed));
        StartCoroutine(HideAfterDelay());
    }

    public void Hide()
    {
        isActive = false;
        StopAllCoroutines();
        StartCoroutine(MoveTo(hiddenPosition,hideSpeed));
    }

    System.Collections.IEnumerator MoveTo(Vector3 target, float speed) // Enumerator make it moves like animation
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    System.Collections.IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(stayDuration);
        Hide();
    }

    //When hit
    public void OnHit()
    {
        Hide();
    }
}
