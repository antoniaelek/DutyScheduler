
var User = React.createClass({

	propTypes: {
		user: React.PropTypes.object.isRequired,
		onDeleteClicked: React.PropTypes.func.isRequired,
		onSetAsAdminClicked: React.PropTypes.func.isRequired,
		onDataUpdate: React.PropTypes.func.isRequired
	},

	getInitialState: function () {
		return {
			isEditMode: false,
		};
	},

	onEditClicked: function () {
		this.setState({ isEditMode: true });
	},

	onUserUpdated: function () {
		this.props.onDataUpdate();
		this.setState({ isEditMode: false });
	},

	onCancelEdit: function () {
		this.setState({ isEditMode: false });
	},

	renderEditMode: function () {
		if (!this.state.isEditMode)
			return null;

		return (
			<UserEdit mode="edit" user={this.props.user} onChangesSaved={this.onUserUpdated} onCancelClicked={this.onCancelEdit} />
		);
	},

	render: function () {

		var user = this.props.user;
		var isCurrentUser = CurrentUser.username === user.username.toString();

		var setAdminButton = null;
		var editButton = null;
		var deleteButton = null;

		if (user.username !== CurrentUser.username && CurrentUser.isAdmin) {
			setAdminButton = (
				<div>
					<Icon className="userIcon" onClick={this.props.onSetAsAdminClicked.bind(null, user)} icon="id-badge" />
				</div>
			);
		}
		if (user.username === CurrentUser.username) {
			editButton = (
				<div>
					<Icon className="userIcon" onClick={this.onEditClicked} icon="pencil" />
				</div>
			);
		}

		if (user.username === CurrentUser.username || CurrentUser.isAdmin) {
			deleteButton = (
				<div>
					<Icon className="userIcon" onClick={this.props.onDeleteClicked.bind(null, user)} icon="trash" />
				</div>
			);
		}

		var userElement = (
			<div className="userContainer">
				<div className="userElement">
					<div className="name"><span className="label">Name:</span> {user.name} {user.lastName}</div>
					<div className="office"><span className="label">Office:</span> {user.office}</div>
					<div className="phone"><span className="label">Phone:</span> { user.phone}</div>
					<div className="email"><span className="label">E-mail:</span> { user.email}</div>
				</div>
				<div className="userIcons">
					{setAdminButton}
					{editButton}
					{deleteButton}
				</div>
			</div>
		);

		var className = "userInfo";
		if (isCurrentUser) {
			className += " currentUser";
		}
		if (user.isAdmin) {
			className += " admin";
		}

		return (
			<div>
				<div className={className }>
					{userElement}
				</div>
				{this.renderEditMode()}
			</div>
		);
	}

});
