var UserView = React.createClass({

	getInitialState: function () {
		return {
			users: []
		};
	},

	componentDidMount: function () {
		this.load();
	},

	onDeleteUserClicked: function (user) {
		API.Ajax("DELETE", "api/User/" + user.username)
		.done(this.load)
		.fail(reportError)
	},

	onSetAsAdminClicked: function (user) {
		API.Ajax("PUT", "api/User/admin/" + user.username, { setAdmin: !user.isAdmin })
		.done(this.load)
		.fail(reportError)
	},

	load: function () {
		API.Ajax("GET", "api/User")
			.done(function (users) {
				this.setState({ users: users });
			}.bind(this))
			.fail(reportError);
	},

	userChangesSaved: function () {
		this.load();
	},

	render: function () {

		var users = this.state.users.map(function (u) {
			return <User onDataUpdate={this.load} onDeleteClicked={this.onDeleteUserClicked} onSetAsAdminClicked={this.onSetAsAdminClicked} key={u.username} user={u} />
		}.bind(this));

		var addNewUser = null;
		if (CurrentUser.isAdmin) {
			addNewUser = (
				<UserEdit mode="add" onChangesSaved={this.userChangesSaved} />
			);
		}

		return (
			<div>
				{users}
				{addNewUser}
			</div>
		);
	}

});