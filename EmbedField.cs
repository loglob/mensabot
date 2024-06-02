namespace mensabot
{
	public record EmbedField(string Name, string Value, bool Inline = true)
	{
		public static implicit operator EmbedField((string name, object value) t)
			=> new(t.name, t.value.ToString()!);

		public static implicit operator EmbedField((string name, object value, bool inline) t)
			=> new(t.name, t.value.ToString()!, t.inline);
	}

	public record EmbedImage(string url);
}