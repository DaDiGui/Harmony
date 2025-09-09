using System.IO;
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail;

//
// Resumen:
//     Representa los datos adjuntos de un correo electrónico.
public class Attachment : AttachmentBase
{
    private ContentDisposition contentDisposition = new ContentDisposition();

    private Encoding nameEncoding;

    //
    // Resumen:
    //     Obtiene la disposición de contenido MIME de estos datos adjuntos.
    //
    // Devuelve:
    //     Objeto System.Net.Mime.ContentDisposition que proporciona la información de presentación
    //     de estos datos adjuntos.
    public ContentDisposition ContentDisposition => contentDisposition;

    //
    // Resumen:
    //     Obtiene o establece el valor de nombre para el tipo de contenido MIME del tipo
    //     de contenido asociado a estos datos adjuntos.
    //
    // Devuelve:
    //     Objeto System.String que contiene el valor para el tipo de contenido name representado
    //     por la propiedad System.Net.Mime.ContentType.Name.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor especificado para una operación Set es null.
    //
    //   T:System.ArgumentException:
    //     El valor especificado para una operación de establecimiento es System.String.Empty
    //     ("").
    public string Name
    {
        get
        {
            return base.ContentType.Name;
        }
        set
        {
            base.ContentType.Name = value;
        }
    }

    //
    // Resumen:
    //     Especifica la codificación para System.Net.Mail.AttachmentSystem.Net.Mail.Attachment.Name.
    //
    //
    // Devuelve:
    //     Valor System.Text.Encoding que especifica el tipo de codificación de nombres.
    //     El valor predeterminado se toma del nombre de los datos adjuntos.
    public Encoding NameEncoding
    {
        get
        {
            return nameEncoding;
        }
        set
        {
            nameEncoding = value;
        }
    }

    //
    // Resumen:
    //     Inicializa una nueva instancia de la clase System.Net.Mail.Attachment con la
    //     cadena de contenido especificada.
    //
    // Parámetros:
    //   fileName:
    //     Objeto System.String que contiene una ruta de acceso al archivo que se va a utilizar
    //     para crear estos datos adjuntos.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de fileName es null.
    //
    //   T:System.ArgumentException:
    //     fileName está vacía.
    public Attachment(string fileName)
        : base(fileName)
    {
        InitName(fileName);
    }

    //
    // Resumen:
    //     Inicializa una nueva instancia de la clase System.Net.Mail.Attachment con la
    //     cadena de contenido y la información de tipo MIME que se hayan especificado.
    //
    //
    // Parámetros:
    //   fileName:
    //     Objeto System.String que incluye el contenido de estos datos adjuntos.
    //
    //   mediaType:
    //     Objeto System.String que contiene la información Content-Header MIME de estos
    //     datos adjuntos. Este valor puede ser null.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de fileName es null.
    //
    //   T:System.FormatException:
    //     mediaType no tiene el formato correcto.
    public Attachment(string fileName, string mediaType)
        : base(fileName, mediaType)
    {
        InitName(fileName);
    }

    //
    // Resumen:
    //     Inicializa una nueva instancia de la clase System.Net.Mail.Attachment con la
    //     cadena de contenido y el objeto System.Net.Mime.ContentType que se hayan especificado.
    //
    //
    // Parámetros:
    //   fileName:
    //     Objeto System.String que contiene una ruta de acceso al archivo que se va a utilizar
    //     para crear estos datos adjuntos.
    //
    //   contentType:
    //     Objeto System.Net.Mime.ContentType que describe los datos de fileName.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de fileName es null.
    //
    //   T:System.ArgumentException:
    //     mediaType no tiene el formato correcto.
    public Attachment(string fileName, ContentType contentType)
        : base(fileName, contentType)
    {
        InitName(fileName);
    }

    //
    // Resumen:
    //     Inicializa una nueva instancia de la clase System.Net.Mail.Attachment con la
    //     cadena y el tipo de contenido especificados.
    //
    // Parámetros:
    //   contentStream:
    //     Objeto System.IO.Stream legible que incluye el contenido de estos datos adjuntos.
    //
    //
    //   contentType:
    //     Objeto System.Net.Mime.ContentType que describe los datos de contentStream.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de contentType es null. O bien El valor de contentStream es null.
    public Attachment(Stream contentStream, ContentType contentType)
        : base(contentStream, contentType)
    {
    }

    //
    // Resumen:
    //     Inicializa una nueva instancia de la clase System.Net.Mail.Attachment con la
    //     secuencia y el nombre especificados.
    //
    // Parámetros:
    //   contentStream:
    //     Objeto System.IO.Stream legible que incluye el contenido de estos datos adjuntos.
    //
    //
    //   name:
    //     Objeto System.String que contiene el valor de la propiedad System.Net.Mime.ContentType.Name
    //     del objeto System.Net.Mime.ContentType asociado a estos datos adjuntos. Este
    //     valor puede ser null.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de contentStream es null.
    public Attachment(Stream contentStream, string name)
        : base(contentStream)
    {
        Name = name;
    }

    //
    // Resumen:
    //     Inicializa una nueva instancia de la clase System.Net.Mail.Attachment con la
    //     cadena, el nombre y la información de tipo MIME que se hayan especificado.
    //
    // Parámetros:
    //   contentStream:
    //     Objeto System.IO.Stream legible que incluye el contenido de estos datos adjuntos.
    //
    //
    //   name:
    //     Objeto System.String que contiene el valor de la propiedad System.Net.Mime.ContentType.Name
    //     del objeto System.Net.Mime.ContentType asociado a estos datos adjuntos. Este
    //     valor puede ser null.
    //
    //   mediaType:
    //     Objeto System.String que contiene la información Content-Header MIME de estos
    //     datos adjuntos. Este valor puede ser null.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de stream es null.
    //
    //   T:System.FormatException:
    //     mediaType no tiene el formato correcto.
    public Attachment(Stream contentStream, string name, string mediaType)
        : base(contentStream, mediaType)
    {
        Name = name;
    }

    //
    // Resumen:
    //     Crea datos adjuntos a un mensaje utilizando el contenido de la cadena especificada
    //     y el objeto System.Net.Mime.ContentType especificado.
    //
    // Parámetros:
    //   content:
    //     Objeto System.String que incluye el contenido de estos datos adjuntos.
    //
    //   contentType:
    //     Objeto System.Net.Mime.ContentType que representa el encabezado Content-Type
    //     MIME (intercambio multipropósito de correo Internet) que se va a utilizar.
    //
    // Devuelve:
    //     Objeto de tipo System.Net.Mail.Attachment.
    public static Attachment CreateAttachmentFromString(string content, ContentType contentType)
    {
        if (content == null)
        {
            throw new ArgumentNullException("content");
        }

        MemoryStream memoryStream = new MemoryStream();
        StreamWriter streamWriter = new StreamWriter(memoryStream);
        streamWriter.Write(content);
        streamWriter.Flush();
        memoryStream.Position = 0L;
        return new Attachment(memoryStream, contentType)
        {
            TransferEncoding = TransferEncoding.QuotedPrintable
        };
    }

    //
    // Resumen:
    //     Crea datos adjuntos a un mensaje utilizando el contenido de la cadena especificada
    //     y el nombre para el tipo de contenido MIME especificado.
    //
    // Parámetros:
    //   content:
    //     Objeto System.String que incluye el contenido de estos datos adjuntos.
    //
    //   name:
    //     Valor de nombre para el tipo de contenido MIME del tipo de contenido asociado
    //     a estos datos adjuntos.
    //
    // Devuelve:
    //     Objeto de tipo System.Net.Mail.Attachment.
    public static Attachment CreateAttachmentFromString(string content, string name)
    {
        if (content == null)
        {
            throw new ArgumentNullException("content");
        }

        MemoryStream memoryStream = new MemoryStream();
        StreamWriter streamWriter = new StreamWriter(memoryStream);
        streamWriter.Write(content);
        streamWriter.Flush();
        memoryStream.Position = 0L;
        return new Attachment(memoryStream, new ContentType("text/plain"))
        {
            TransferEncoding = TransferEncoding.QuotedPrintable,
            Name = name
        };
    }

    //
    // Resumen:
    //     Crea los datos adjuntos a un mensaje utilizando el contenido de la cadena especificada,
    //     el nombre de tipo de contenido MIME especificado, la codificación de caracteres
    //     y la información de encabezado MIME de los datos adjuntos.
    //
    // Parámetros:
    //   content:
    //     Objeto System.String que incluye el contenido de estos datos adjuntos.
    //
    //   name:
    //     Valor de nombre para el tipo de contenido MIME del tipo de contenido asociado
    //     a estos datos adjuntos.
    //
    //   contentEncoding:
    //     Una clase System.Text.Encoding. Este valor puede ser null.
    //
    //   mediaType:
    //     Objeto System.String que contiene la información Content-Header MIME de estos
    //     datos adjuntos. Este valor puede ser null.
    //
    // Devuelve:
    //     Objeto de tipo System.Net.Mail.Attachment.
    public static Attachment CreateAttachmentFromString(string content, string name, Encoding contentEncoding, string mediaType)
    {
        if (content == null)
        {
            throw new ArgumentNullException("content");
        }

        MemoryStream memoryStream = new MemoryStream();
        StreamWriter streamWriter = new StreamWriter(memoryStream, contentEncoding);
        streamWriter.Write(content);
        streamWriter.Flush();
        memoryStream.Position = 0L;
        return new Attachment(memoryStream, name, mediaType)
        {
            TransferEncoding = MailMessage.GuessTransferEncoding(contentEncoding),
            ContentType =
            {
                CharSet = streamWriter.Encoding.BodyName
            }
        };
    }

    private void InitName(string fileName)
    {
        if (fileName == null)
        {
            throw new ArgumentNullException("fileName");
        }

        Name = Path.GetFileName(fileName);
    }
}
