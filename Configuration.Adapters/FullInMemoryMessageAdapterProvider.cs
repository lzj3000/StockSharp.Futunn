namespace StockSharp.Configuration
{
	using System;
	using System.Collections.Generic;

	using Ecng.Common;

	using StockSharp.Futunn;
    using StockSharp.Logging;
    using StockSharp.Messages;

    /// <summary>
    /// In memory configuration message adapter's provider.
    /// </summary>
    public class FullInMemoryMessageAdapterProvider : InMemoryMessageAdapterProvider
	{
		/// <summary>
		/// Initialize <see cref="FullInMemoryMessageAdapterProvider"/>.
		/// </summary>
		/// <param name="currentAdapters">All currently available adapters.</param>
		public FullInMemoryMessageAdapterProvider(IEnumerable<IMessageAdapter> currentAdapters)
			: base(currentAdapters)
		{
		}

		/// <inheritdoc />
		protected override IEnumerable<Type> GetAdapters()
		{
			var adapters = new HashSet<Type>(base.GetAdapters());

			foreach (var func in _standardAdapters.Value)
			{
				try
				{
					var type = func();

					//if (type == typeof(QuikDdeAdapter) || type == typeof(QuikTrans2QuikAdapter))
					//	adapters.Remove(type);
					//else
					adapters.Add(type);
				}
				catch (Exception e)
				{
					e.LogError();
				}
			}

			return adapters;
		}

		private static readonly Lazy<Func<Type>[]> _standardAdapters = new Lazy<Func<Type>[]>(() => new[]
		{
			(Func<Type>)(() => typeof(FutunnMessageAdapter))
		});

		/// <inheritdoc />
		//public override IMessageAdapter CreateTransportAdapter(IdGenerator transactionIdGenerator) => new FixMessageAdapter(transactionIdGenerator);
	}
}