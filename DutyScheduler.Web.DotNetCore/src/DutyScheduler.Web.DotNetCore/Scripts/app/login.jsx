
var Login = React.createClass({

	getInitialState: function () {
		return {
			userName: "",
			password: "",
			error: ""
		};
	},

	onClick: function (pref) {
		this.setState({ prefered: pref });
	},

	onUsernameChanged: function (e) {
		this.setState({ userName: e.target.value });
	},

	onPasswordChanged: function (e) {
		this.setState({ password: e.target.value });
	},

	tryLogin: function () {
		API.Ajax("POST", "api/Session", { userName: this.state.userName, password: this.state.password })
			.done(function (response) {
				this.props.onLoggedIn(response);
			}.bind(this))
			.fail(function (response) {
				reportError(response);
				this.setState({ userName: "", password: "", error: "Invalid email or password." });
			}.bind(this));
	},

	render: function () {

		return (
			<div className="loginContainer">
				<div>Please, log in.</div>
				<input type="text" placeholder="Username" value={this.state.userName} onChange={this.onUsernameChanged} />
				<input type="password" placeholder="Password" value={this.state.password} onChange={this.onPasswordChanged} />
				<button onClick={this.tryLogin}>Login</button>
				<div className="errorText">{this.state.error}</div>
			</div>
		);
	}
});