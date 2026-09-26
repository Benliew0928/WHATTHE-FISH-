using WhatTheFish;
using UnityEngine;
public static partial class ProjectBuilder {
 static IslandLayout ReadFishing()=>RefinedIslandBuilder.Read("Fishing");
 static GameObject BuildFishing()=>RefinedIslandBuilder.Build("Fishing");
}
