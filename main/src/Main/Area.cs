using Godot;
using System;

public class Area : Node
{
	public override void _Ready()
	{
		Global.screenSize = GetViewport().Size;

		var Composer = Global.composer;
		Composer.GotoScene(Composer.scenes["mainmenu"],false,"fade",true);
	}
}
