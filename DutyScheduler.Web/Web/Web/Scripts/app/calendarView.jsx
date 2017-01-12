var CalendarView = React.createClass({

	getInitialState: function () {
		var today = moment();

		return {
			selectedMonth: today.month(), // zero-indexed
			selectedYear: today.year(),
			days: null,
			selectedDayNumber: null,
		};
	},

	componentDidMount: function () {
		this.load(this.state.selectedYear, this.state.selectedMonth, true);
	},

	load: function (year, month, clearSelected) {
		API.Ajax("GET", "api/Calendar/year=" + year + "&month=" + (month + 1))
			.done(function (response) {

				response.forEach(function (day) { day.date = moment(day.date); });

				var newState = {
					days: response,
					selectedMonth: month,
					selectedYear: year,
				};

				if (clearSelected)
					newState.selectedDayNumber = null;

				this.setState(newState);

			}.bind(this))
			.fail(reportError);
	},

	onDayUpdate: function(){
		this.load(this.state.selectedYear, this.state.selectedMonth);
	},

	renderCellContent: function (day) {

		var dayOfWeek = day.date.day();
		var dayNumber = day.date.date();

		if (day.type === "non-working")
			return <div className="dayNumber nonworking">{dayNumber}</div>;

		if (day.type === "holiday")
			return <div className="dayNumber holiday">{dayNumber}</div>;

		var shorterDayClassName = "";
		if (day.type === "special")
			shorterDayClassName = " shortDayIcon";

		if (day.scheduled !== null) {
			var icon = null;
			var replacementIcon = null;
			if (day.isReplaceable)
				replacementIcon = <Icon className="replacement" icon="repeat" />;

			if (day.scheduled.username === CurrentUser.username) {
				icon = <Icon icon="user-circle-o" />;
			}

			return <div className={"dayNumber" + shorterDayClassName}>{dayNumber}<div>{icon}{replacementIcon}</div></div>;
		}

		if (day.date.isAfter(moment())) {
			var icon = null;
			if (day.isPrefered === true) {
				icon = <Icon className="prefered" icon="thumbs-up" />;
			}
			else if (day.isPrefered === false) {
				icon = <Icon className="notprefered" icon="thumbs-down" />;
			}

		return <div className={"dayNumber" + shorterDayClassName}>{dayNumber}<div>{icon}</div></div>;
		}

		//var icon = Math.random() < 0.2 ? <Icon icon="user-circle-o" /> : null;
		var icon = null;
		return <div className="dayNumber">{dayNumber}<div>{icon}</div></div>;
	},

	onDayClicked: function (dayNumber) {
		this.setState({ selectedDayNumber: dayNumber });
	},
	
	renderRows: function () {
		// Sunday is first day
		var month = moment()
			.year(this.state.selectedYear).month(this.state.selectedMonth).date(1)
			.hour(0).minute(0).second(0);

		var daysInMonth = month.daysInMonth();

		var firstDayOfMonth = month.day();
		var offsets = [6, 0, 1, 2, 3, 4, 5];
		var offset = offsets[firstDayOfMonth];

		var rows = [];

		var dayIndex = 1;
		for (var row = 0; row < 6; row++) {
			var columnElements = [];

			for (var col = 0; col < 7; col++) {

				var dayNumberText = "";

				var dayNumber = dayIndex - offset;
				var content = null;
				if (dayNumber > 0 && dayNumber <= daysInMonth) {
					dayNumberText = dayIndex - offset;

					var day = this.state.days[dayNumber - 1];
					content = this.renderCellContent(day, dayNumber);
				}

				var tdClassName = "";
				if (this.state.selectedDayNumber === dayNumber)
					tdClassName = "selected";

				var columnKey = "cell" + col;
				var columnCell = (<td className={tdClassName} onClick={this.onDayClicked.bind(this, dayNumber)} key={columnKey }>{content}</td>);

				columnElements.push(columnCell);

				if (dayNumber == daysInMonth) {
					row += 1000; // Force outer loop exit
					break;
				}

				dayIndex++;
			}
			var rowKey = "row" + row;
			var rowElement = <tr key={rowKey }>{columnElements}</tr>;
			rows.push(rowElement);
		}

		return rows;
	},

	onPreviousMonthClicked: function () {
		var year = this.state.selectedYear;
		var month = this.state.selectedMonth - 1;
		if (month < 0) {
			month = 11;
			year--;
		}

		this.load(year, month, true);
	},

	onNextMonthClicked: function () {
		var year = this.state.selectedYear;
		var month = this.state.selectedMonth + 1;
		if (month > 11) {
			month = 0;
			year++;
		}

		this.load(year, month, true);
	},

	renderDayPanel: function () {

		if (!this.state.selectedDayNumber)
			return null;

		var dayIndex = this.state.selectedDayNumber - 1;
		if (!this.state.days || dayIndex < 0 || this.state.days.length <= dayIndex)
			return null;

		var day = this.state.days[dayIndex];

		return <DayPanel onDayUpdate={this.onDayUpdate} day={day } />;
	},

	render: function () {

		if (this.state.days === null)
			return null;

		var rows = this.renderRows();

		return (
			<div className="calendarView">
				<div className="calendarContainer">
					<div className="monthControls">
						<div className="button" onClick={this.onPreviousMonthClicked}>
							<Icon icon="chevron-left" />
						</div>
						<span className="month">{this.state.selectedMonth + 1} / {this.state.selectedYear}</span>
						<div className="button" onClick={this.onNextMonthClicked}>
							<Icon icon="chevron-right" />
						</div>
					</div>
					<table className="daysGrid">
						<thead>
							<tr className="row header">
								<td>Mon</td>
								<td>Tue</td>
								<td>Wed</td>
								<td>Thu</td>
								<td>Fri</td>
								<td>Sat</td>
								<td>Sun</td>
							</tr>
						</thead>
						<tbody>
							{rows}
						</tbody>
					</table>
				</div>
				<div className="dayContent">{this.renderDayPanel()}</div>
			</div>
		);
	}
});