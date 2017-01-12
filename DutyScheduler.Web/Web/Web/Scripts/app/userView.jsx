var UserView = React.createClass({

	getInitialState: function () {
		return {
			users: []
		};
	},

	componentDidMount: function () {
		this.load();
	},

	load: function() {
		API.Ajax("GET", "api/User")
			.done(function (users) {
				this.setState({ users: users });
			}.bind(this))
			.fail(reportError);
	},

	render: function () {

		var users = this.state.users.map(function (u) {
			return <User key={u.username} user={u} />
			});

		return (<div>{users}</div>);
	}

});