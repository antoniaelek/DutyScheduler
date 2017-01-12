
var User = React.createClass({

	render: function () {

		var user = this.props.user;
		var isCurrentUser = CurrentUser.username === user.username.toString();

		var userElement = (
			<div>
				<div className="name"> {"Name:"} {user.name} {user.lastName}</div>
				<div className="office"> {"Office:"} {user.office}</div>
				<div className="phone"> { "Phone:"} { user.phone}</div>
				<div className="email"> { "E-mail:"} { user.email}</div>
			</div>
		);

		var className = "userInfo";
		if (isCurrentUser) {
			className += " currentUser";
		}
		if (user.isAdmin) {
			className += " admin";
		}

		return (<div className={className }>{userElement} </div>);
	}

});
