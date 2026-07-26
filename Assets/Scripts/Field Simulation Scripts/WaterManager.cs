using UnityEngine;

public class WaterManager : MonoBehaviour
{
    [SerializeField] Material container;
    [SerializeField] float startvalue;
    [SerializeField] float endvalue;

    [SerializeField] Material shreder;
    [SerializeField] float startvalue1;
    [SerializeField] float endvalue1;

    [SerializeField] Material pipe;
    [SerializeField] float startvalue2;
    [SerializeField] float endvalue2;

    [SerializeField] float Speed;
    public string n;

    float cr, cr1, cr2;

    void Start()
    {
        cr = startvalue;
        cr1 = startvalue1;
        cr2 = startvalue2;

        container.SetFloat(n, cr);
        shreder.SetFloat(n, cr1);
        pipe.SetFloat(n, cr2);
    }

    void Update()
    {
        cr = Mathf.MoveTowards(cr, endvalue, Speed * Time.deltaTime);
        cr1 = Mathf.MoveTowards(cr1, endvalue1, Speed * Time.deltaTime);
        cr2 = Mathf.MoveTowards(cr2, endvalue2, Speed * Time.deltaTime);

        container.SetFloat(n, cr);
        shreder.SetFloat(n, cr1);
        pipe.SetFloat(n, cr2);
    }
}
