
var Shift = React.createClass({

	render: function () {

		var shift = this.props.shift;
		var isOwnShift = localStorage.currentUserId === shift.user.id.toString();

		var className = "shift";
		var button = <div></div>;
		if (isOwnShift) {
			className += " currentUser";
			button = <div className="panelButton">Zamijeni termin</div>
		}

		return (
				<div className={className}>
				<div className="header">
					{shift.date.format("YYYY-MM-DD")} ({shift.day})
				</div>
				<div className="content">
					{shift.user.name} | {shift.user.phone} | {shift.user.room}
					{button}
				</div>
				</div>
		);
	}
});