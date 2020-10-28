namespace mensabot
{
	public class EmbedField
	{
		public string name;
		public string value;
		public bool inline = true;

		public static implicit operator EmbedField((string name, object value) t)
			=> new EmbedField{ name = t.name, value = t.value.ToString() };

		public static implicit operator EmbedField((string name, object value, bool inline) t)
			=> new EmbedField{ name = t.name, value = t.value.ToString(), inline = t.inline };
	}

	public class EmbedImage
	{
		public string url;
	}
}