namespace CommonInterfaces.DTO
{
    public enum DataTransferStatus
    {
        Information,
        Success,
        Failure,
        Warning,
        Fatal
    }

    public interface IWrapper<T>
    {
        public T? Data { get; set; }
        public string? Title { get; set; }
        public DataTransferStatus Status { get; set; }
        public string Message { get; set; }

    }
}
