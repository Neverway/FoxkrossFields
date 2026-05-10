public interface ITileObjectReceiver
{
    void ReceiveData(TileObjectData data, float elapsedSeconds = 0f);
    TileObjectData ProvideData();
}