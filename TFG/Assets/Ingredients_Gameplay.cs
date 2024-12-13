using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Ingredients_Gameplay : MonoBehaviour
{
    public string nameIngredient_str = "";
    public List<string> nameIngredient_Variants_str = new List<string>(); //Meter plurales, palabras alternativas, etc.
    public Sprite ingredient_sprite;    
    public GameObject thisGameObject_go = null; //Lo utilizo para luego instanciarlo.
    public int price_i = 0; //Solo productos finales tienen precio,es lo que darán al entregarlo.
    int priceReal_i = 0; //Por si hay que aplicar bonus o nerfeos.

    //Todo:Si hacemos un diccionario poner que tipo de items puede afectarle Clave Palabra , clase: tipo(ingrediente,...) afectado po tareas(cortar,freir...)

    [Header("")]
    public bool usingIt_b = false;

    [Header("")]
    public List<string> taskAffectedIt = new List<string>();//**Me he dado cuenta que todos los ingredientes se deben poder Traer, a lo mejor ignorar esa tarea? O hacerlo Bool si hay una excepción?
    //*Pos: Tal ve usar diccionarios para cuando cambiamos de idioma?

    public int countInItem_i = 0;//Usarlo para los objetos en cocina y el Npc lleva un bulto aquí pone cuantos lleva en realidad, como los sacos de monedas en videojuegos. //También para las recetas
    public bool productStart_b = false;
    public bool productFinal_b = false;
    public bool productFinal_WithPlate_b = false;//**Se usará para aceptar o no las comidas para obtener el dinero. Esta debe ser true,y por lo tanto antes debe ser plato final true.

    //
    public List<GameObject> ingredientsToAffectIfIwasSaid_ = new List<GameObject>(); //Debido como hablamos los humanos talvez decimos freír 'carne' y no freir 'carne cortada', pues tenemos que preparar el programa para esto.

    //Todo:Poner contador para que la comida se vaya a poner mala.

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
