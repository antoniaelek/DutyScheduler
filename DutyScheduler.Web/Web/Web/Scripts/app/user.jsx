
var User = React.createClass({

	propTypes: {
		onDeleteClicked: React.PropTypes.func.isRequired,
		onSetAsAdminClicked: React.PropTypes.func.isRequired
	},

	render: function () {

		var user = this.props.user;
		var isCurrentUser = CurrentUser.username === user.username.toString();

		var iconButtons = null;
		if (CurrentUser.isAdmin) {
			var adminIcon = (
				<div>
					<Icon className="userIcon" onClick={this.props.onSetAsAdminClicked.bind(null, user)} icon="id-badge"/>
				</div>
			);
			if (CurrentUser.username === user.username)
			{
				adminIcon = null;
			}

			iconButtons = (
				<div className="userIcons">
					<div>
						<Icon className="userIcon" onClick={this.props.onDeleteClicked.bind(null, user)} icon="trash"/>
					</div>
					{adminIcon}
				</div>
			);
		}

		var userElement = (
			<div className="userContainer">
				<div className="userElement">
					<div className="name"> {"Name:"} {user.name} {user.lastName}</div>
					<div className="office"> {"Office:"} {user.office}</div>
					<div className="phone"> { "Phone:"} { user.phone}</div>
					<div className="email"> { "E-mail:"} { user.email}</div>
				</div>
				{iconButtons}
			</div>
		);

		var className = "userInfo";
		if (isCurrentUser) {
			className += " currentUser";
		}
		if (user.isAdmin) {
			className += " admin";
		}

		return (<div className={className}>{userElement} </div>);
	}

});
