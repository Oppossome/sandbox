using Sandbox;
using System;
using System.Linq;
using System.Collections.Generic;

partial class Inventory : BaseInventory
{
	static Dictionary<Type, int> ToolOrder = new()
	{
		{ typeof( PhysGun ), 0 },
		{ typeof( GravGun ), 1 },
		{ typeof( Tool ), 2 },
		{ typeof( Flashlight ), 3 },
		{ typeof( Pistol ), 4 },
		{ typeof( Fists ), 5 }
	};


	public Inventory( Player player ) : base( player )
	{
	}

	public override bool CanAdd( Entity entity )
	{
		if ( !entity.IsValid() )
			return false;

		if ( !base.CanAdd( entity ) )
			return false;

		return !IsCarryingType( entity.GetType() );
	}

	public override bool Add( Entity entity, bool makeActive = false )
	{
		if ( !entity.IsValid() )
			return false;

		if ( IsCarryingType( entity.GetType() ) )
			return false;

		return base.Add( entity, makeActive );
	}

	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x?.GetType() == t );
	}

	public void SortItems()
	{
		this.List = this.List.OrderBy( x =>
		{
			if ( ToolOrder.TryGetValue( x.GetType(), out int order ) )
				return order;

			return 10;
		} ).ToList();
	}

	public override bool Drop( Entity ent )
	{
		if ( !Host.IsServer )
			return false;

		if ( !Contains( ent ) )
			return false;

		if( ent is BaseCarriable carriable)
			carriable.OnCarryDrop( Owner );

		return ent.Parent == null;
	}
}
