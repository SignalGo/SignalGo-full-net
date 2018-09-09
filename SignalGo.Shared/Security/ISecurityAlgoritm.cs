namespace SignalGo.Shared.Security
{
    public interface ISecurityAlgoritm
    {
        byte[] Encrypt(byte[] bytes);
        byte[] Decrypt(byte[] bytes);
    }
}
