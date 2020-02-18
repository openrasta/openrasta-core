namespace Tests.DI.SpeedyGonzales
{
  public class Quest
  {
    public Hero Hero { get; }
    public Artifact Artifact { get; }

    public Quest(Hero hero, Artifact artifact)
    {
      Hero = hero;
      Artifact = artifact;
    }
  }
}