var chartData = [(1, 4), (2, 5), (3, 10)];
var chartOptions = {};

var Statistics = React.createClass({

	getInitialState: function () {
		return {
			currentYear: moment().year(),
			isLoading: false,
			chartData: {
			},
			tableData: {
				shifts: [],
				replacing: [],
				replaced: []
			}
		};
	},

	componentDidMount: function () {
		var canvas = document.getElementById("monthlyChart");
		this.chart = new Chart(canvas, {
			type: "bar",
			data: {
				labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
				datasets: [{
					label: "Ordinary",
					data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
					backgroundColor: 'rgb(99, 132, 255)'
				}, {
					label: "Special",
					data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
					backgroundColor: 'rgb(255, 99, 132)'
				}, {
					label: "Total",
					data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
					backgroundColor: 'rgb(99, 99, 99)'
				}]
			},
			options: {
				legend: { position: "bottom" },
				maintainAspectRatio: false,
				responsive: true,
				scales: {
					yAxes: [{
						type: "linear",
						ticks: {
							stepSize: 1
						}
					}]
				}
			}
		});

		this.load();
	},

	onDataLoaded: function (data) {
		this.setState({
			chartData: this.calculateChartData(data),
			tableData: this.calculateTableData(data)
		}, function () {
			this.chart.data.datasets[0].data = this.state.chartData.ordinary;
			this.chart.data.datasets[1].data = this.state.chartData.special;
			this.chart.data.datasets[2].data = this.state.chartData.total;
			this.chart.update();
		});
	},

	load: function () {
		API.Ajax("GET", "api/Statistics/year=" + this.state.currentYear)
		.done(this.onDataLoaded)
		.fail(reportError);
	},

	onYearModifyButtonClicked: function (direction) {

		var chosenYear = this.state.currentYear + direction;
		this.setState({ currentYear: chosenYear, isLoading: true }, function () {
			this.load();
		});
	},

	calculateChartData: function (newState) {

		var chartData = {
			ordinary: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
			special: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
			total: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
		};

		newState.shifts.forEach(function (shift) {
			var monthIndex = moment(shift.date).month();

			if (shift.type === "ordinary" || shift.type === "special")
				chartData[shift.type][monthIndex]++;
		});

		for (var i = 0; i < 12; i++) {
			chartData.total[i] = chartData.ordinary[i] + chartData.special[i];
		}

		return chartData;
	},

	calculateTableData: function (newState) {

		var data = {
			shifts: { ordinary: 0, special: 0 },
			replacing: { ordinary: 0, special: 0 },
			replaced: { ordinary: 0, special: 0 },
		};

		newState.shifts.forEach(function (shift) {
			if (shift.type === "ordinary" || shift.type === "special")
				data.shifts[shift.type]++;
		});

		newState.replacing.forEach(function (shift) {
			if (shift.type === "ordinary" || shift.type === "special")
				data.replacing[shift.type]++;
		});

		newState.replaced.forEach(function (shift) {
			if (shift.type === "ordinary" || shift.type === "special")
				data.replaced[shift.type]++;
		});

		return data;
	},


	render: function () {

		var tableData = this.state.tableData;

		return (
			<div className="statisticsView">

				<div className="monthControls">
						<div className="button" onClick={this.onYearModifyButtonClicked.bind(this, -1)}>
							<Icon icon="chevron-left" />
						</div>
						<span className="month">{this.state.currentYear}</span>
						<div className="button" onClick={this.onYearModifyButtonClicked.bind(this, 1)}>
							<Icon icon="chevron-right" />
						</div>
				</div>

				<div className="title">Work report</div>
					<table>
						<thead>
							<tr>
								<th className="emptyCell"></th>
								<th>Ordinary</th>
								<th>Special</th>
								<th>Total</th>
							</tr>
						</thead>
						<tbody>
							<tr>
								<td className="label">Completed</td>
								<td>{tableData.shifts.ordinary}</td>
								<td>{tableData.shifts.special}</td>
								<td className="total">{tableData.shifts.ordinary + tableData.shifts.special}</td>
							</tr>
							<tr>
								<td className="label">Replaced someone</td>
								<td>{tableData.replacing.ordinary}</td>
								<td>{tableData.replacing.special}</td>
								<td className="total">{tableData.replacing.ordinary + tableData.replacing.special}</td>
							</tr>
							<tr>
								<td className="label">Replaced by someone</td>
								<td>{tableData.replaced.ordinary}</td>
								<td>{tableData.replaced.special}</td>
								<td className="total">{tableData.replaced.ordinary + tableData.replaced.special}</td>
							</tr>
						</tbody>
					</table>
				<div className="monthlyChartContainer">
					<div className="title">By months</div>
						<canvas id="monthlyChart"></canvas>
				</div>
			</div>
		);
	}
});