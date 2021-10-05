// Copyright (c) 2021 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using JetBrains.Annotations;

namespace SIL.Windows.Forms.Miscellaneous
{
	[PublicAPI]
	public static class GraphicsManager
	{
		public enum GtkVersion
		{
			Gtk2,
			Gtk3
		}

		public const GtkVersion GTK2 = GtkVersion.Gtk2;
		public const GtkVersion GTK3 = GtkVersion.Gtk3;

		public static GtkVersion GtkVersionInUse { get; set; } = GTK2;
	}
}
