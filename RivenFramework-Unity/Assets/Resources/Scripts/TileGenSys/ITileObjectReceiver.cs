public interface ITileObjectReceiver
{
    void ReceiveData(TileObjectData data);
    TileObjectData ProvideData();
}