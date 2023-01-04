namespace EIS.Domain.Entities;

public class Payload
{
    public List<ChangedAttributes> ChangedAttributes { get; set;}
    public object Content { get; set;}
    public Object OldContent { get; set;}
    public string ContenetType { get; set;}
    public string SourceSystemName { get; set;}

    public Payload()
    {
    }

    public Payload(object content)
    {
        Content = content;
    }

    public Payload(object content, string contentType, string sourceSystemName)
    {
        Content = content;
        ContenetType = contentType;
        SourceSystemName = sourceSystemName;
    }

    //public TOutput ConvertContent<TOutput>(object payloadContenet)
    //{
    //    return (TOutput)JsonSerializerUtil.Deserialize(payloadContenet.ToString(), typeof(TOutput));
    //}
}
