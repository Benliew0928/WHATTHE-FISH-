using WhatTheFish;
using UnityEngine;
public static partial class ProjectBuilder {
 static IslandLayout ReadGolf()=>RefinedIslandBuilder.Read("Golf");
 static GameObject BuildGolf()=>RefinedIslandBuilder.Build("Golf");
}
