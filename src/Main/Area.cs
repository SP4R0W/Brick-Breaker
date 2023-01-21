using Godot;
using System;

public class Area : Node
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Global.screenSize = GetViewport().Size;
		
		var Composer = Global.composer;
		Composer.GotoScene(Composer.scenes["mainmenu"],false,"fade",true);
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
