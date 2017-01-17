var UserEdit = React.createClass({

	propTypes: {
		user: React.PropTypes.object,
		onChangesSaved: React.PropTypes.func.isRequired,
		onCancelClicked: React.PropTypes.func,
		mode: React.PropTypes.oneOf(["edit", "add"]).isRequired
	},

	getDefaultProps: function () {
		return {
			user: {
				username: "",
				name: "",
				lastName: "",
				office: "",
				phone: "",
				email: "",
				password: ""
			}
		};
	},

	addNewUser: function () {
		
		API.Ajax("POST", "api/User", this.state.user)
			.done(function () {
				this.setState({ user: Object.assign({}, this.props.user) }, function () {
					this.props.onChangesSaved();
				});
			}.bind(this))
			.fail(reportError);
	},

	editUser: function() {
		API.Ajax("PUT", "api/User", this.state.user)
		.done(function () {
			this.props.onChangesSaved();
		}.bind(this))
		.fail(reportError);
	},


	getInitialState: function () {
		return {
			user: Object.assign({}, this.props.user)
		};
	},

	onButtonClicked: function () {
		if (this.props.mode === "edit") {
			this.editUser();
		}
		else {
			this.addNewUser()
		}
		console.log(this.state.user);

	},

	onInputChanged: function (name, e) {
		var currentState = this.state.user;
		currentState[name] = e.target.value;
		this.setState(currentState);
	},

	onCancelClicked: function () {
		if(this.props.onCancelClicked !== undefined)
			this.props.onCancelClicked();
	},

	render: function () {

		var icon = this.props.mode === "edit" ? "floppy-o" : "plus";
		var buttonText = this.props.mode === "edit" ? "Save" : "Add";

		var userNameControls = (
			<div className="username">
				<span className="label">Username:</span>{this.state.user.username}
			</div>
		);

		if (this.props.mode === "add") {
			userNameControls = (
				<div className="username">
					<span className="label">Username:</span>
					<input onChange={this.onInputChanged.bind(this, "username")} value={this.state.user.username} />
				</div>
			);
		}

		return (
			<div className="userInfo newUser">
				<div className="userContainer">
					<div className="userElement">
						{userNameControls}
						<div className="name">
							<span className="label">Name:</span>
							<input onChange={this.onInputChanged.bind(this, "name")} value={this.state.user.name} />
						</div>
						<div className="lastName">
							<span className="label">Last Name:</span>
							<input onChange={this.onInputChanged.bind(this, "lastName")} value={this.state.user.lastName} />
						</div>
						<div className="office">
							<span className="label">Office:</span>
							<input onChange={this.onInputChanged.bind(this, "office")} value={this.state.user.office} />
						</div>
						<div className="phone">
							<span className="label">Phone:</span>
							<input onChange={this.onInputChanged.bind(this, "phone")} value={this.state.user.phone} />
						</div>
						<div className="email">
							<span className="label">E-mail:</span>
							<input onChange={this.onInputChanged.bind(this, "email")} value={this.state.user.email} />
						</div>
						<div className="password">
							<span className="label">Password:</span>
							<input type="password" onChange={this.onInputChanged.bind(this, "password")} value={this.state.user.password} />
						</div>
					</div>
				</div>
				<div className="button saveButton" onClick={this.onButtonClicked}><Icon icon={icon} />{buttonText}</div>
				<div className="button cancelButton" onClick={this.onCancelClicked}><Icon icon="close" />Cancel</div>
			</div>
		);
	}
});