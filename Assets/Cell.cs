using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {

  public bool active;
  public int x;
  public int y;
  private bool isSelectable = true;
  private Kasuri parentKasuri;

  public void Init(int x, int y, float size, bool active) {
    this.x = x;
    this.y = y;
    transform.localScale = new Vector3(size, size, 1);
    Active = active;
    parentKasuri = transform.parent.GetComponent<Kasuri>();
    isSelectable = !parentKasuri.isChild;
  }

  public bool Active {
    get {
      return active;
    }
    set {
      transform.GetComponent<SpriteRenderer>().color = value ? new Color(0, 0, 0) : new Color(1, 1, 1);
      active = value;
    }
  }

  private void OnMouseOver() {
    if (!active && isSelectable) {
      transform.GetComponent<SpriteRenderer>().color = new Color(0.85f, 0.85f, 0.85f);      
    }
  }

  private void OnMouseExit() {
    if (isSelectable) {
      transform.GetComponent<SpriteRenderer>().color = active ? new Color(0, 0, 0) : new Color(1, 1, 1);      
    }
  }

  private void OnMouseDown() {
    if (isSelectable) {
      parentKasuri.UpdateColumn(x, y);      
    } 
  }
}