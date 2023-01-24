namespace DebeziumPocProject.API.Infastructure.Interfaces
{
    public interface IEventListener
    {
        Dictionary<string, string> RetrieveNextBatch();
        void Commit();
    }
}
