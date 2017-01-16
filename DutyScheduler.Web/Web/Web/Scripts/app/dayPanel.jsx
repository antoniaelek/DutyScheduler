var DayPanel = React.createClass({

	propTypes: function () {
		onDayUpdate: React.PropTypes.func.isRequired
	},

	render: function () {

		var day = this.props.day;
		var content = null;

		var title = "";
		var icon = "";

		if (day.type === "holiday") {
			icon = "flag";
			title = day.name;
			content = <DayHoliday day={day } />;
		}
		else if (day.type === "non-working") {
			icon = "bed";
			title = "Weekend";
			content = <DayWeekend day={day } />;
		}
		else if (day.scheduled) {
			icon = "calendar";

			if (day.scheduled.username === CurrentUser.username) {
				title = "My shift";
				content = <DayMyShift day={day } onDayUpdate={this.props.onDayUpdate} />;
			}
			else {
				title = "Not my shift";
				content = <DayTheirShift day={day} onDayUpdate={this.props.onDayUpdate} />;
			}
		}
		else if (day.date.isAfter(moment())) {
			icon = "thumbs-up";
			title = day.date.format("DD. MM. YYYY.");
			content = <DayPreferences onDayUpdate={this.props.onDayUpdate} day={day } />;
		}

		return (
			<div className="dayPanel">
				<div className="header">
					<Icon icon={icon} />
					<span className="title">{title}</span>
				</div>
				<div className="panelContent">
					{content}
				</div>
			</div>
		);
	}
});
