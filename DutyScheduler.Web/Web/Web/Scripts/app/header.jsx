var Header = React.createClass({

	propTypes: {
		items: React.PropTypes.array,
		onMenuItemClicked: React.PropTypes.func.isRequired,
		onLogout: React.PropTypes.func.isRequired,
		selectedItem: React.PropTypes.object
	},

	onMenuItemClicked: function (item) {
		this.props.onMenuItemClicked(item);
	},

	onLogoutClicked: function () {
		this.props.onLogout();
	},

	render: function () {
		var items = _.map(this.props.items, function (item) {
			var className = "menuItem";
			if (item == this.props.selectedItem)
				className += " selected";

			return <div className={className} key={item.text} onClick={this.onMenuItemClicked.bind(this, item)}>
				<Icon icon={item.icon}/>{item.text}
			</div>
		}.bind(this));

		var loginCorner = null;
		if (this.props.items !== null)
		{
			loginCorner = (
				<div className="loginCorner">
					<div className="currentUser">{CurrentUser.name} {CurrentUser.lastName}</div>
					<Icon onClick={this.onLogoutClicked} className="logout" icon="power-off" />
				</div>
			);	
		}

		return (
			<header>
				<h1>
					<Icon icon="calendar" />
					<span>Shift Scheduler</span>
				</h1>
				<div className="menuItems">{items}</div>
				{loginCorner}
			</header>
		);
	}
});