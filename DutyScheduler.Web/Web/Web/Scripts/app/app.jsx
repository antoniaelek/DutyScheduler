var menuItems = [
	//{ text: "Raspored", content: <ScheduleView />, icon: "calendar" },
	//{ text: "Preferences", content: <PreferenceView />, icon: "calendar-check-o" },
	{ text: "Calendar", content: <CalendarView />, icon: "calendar" },
	{ text: "Statistics", content: <Statistics />, icon: "pie-chart" },
	{ text: "Users", content: <UserView />, icon: "user" },
];

var App = React.createClass({

	getInitialState: function () {
		return {
			attemptLogin: true,
			isLoggedIn: false,
			selectedItem: menuItems[0]
		};
	},

	componentDidMount: function () {
		API.Ajax("GET", "api/Session")
		.done(function (response) {
			CurrentUser = response;
			this.setState({ isLoggedIn: true, attemptLogin: false }), function () {
			};
		}.bind(this))
		.fail(function (response) {
			reportError(response);
			this.setState({ isLoggedIn: false, attemptLogin: false });
		}.bind(this));
	},

	onLogout: function () {
		API.Ajax("DELETE", "api/Session")
		.done(function () {
			CurrentUser = null;
			this.setState({ isLoggedIn: false, attemptLogin: false });
		}.bind(this))
		.fail(reportError)
	},

	onMenuItemClicked: function (item) {
		this.setState({ selectedItem: item });
	},

	onLoggedIn: function (user) {
		CurrentUser = user;
		this.setState({ isLoggedIn: true });
	},

	render: function () {

		if (this.state.attemptLogin)
			return null;

		var view = null;
		if (this.state.selectedItem !== null) {
			view = this.state.selectedItem.content;
		}

		var menu = menuItems;
		var loginControl = null;
		if (!this.state.isLoggedIn) {
			menu = null;
			loginControl = <Login onLoggedIn={this.onLoggedIn} />;
			view = null;
		}

		return (
            <div className="app">
                <Header
						onLogout={this.onLogout} 
						selectedItem={this.state.selectedItem} 
						onMenuItemClicked={this.onMenuItemClicked} 
						items={menu}/>
                <div className="mainPageContent">
					{loginControl}
					{view}
				</div>
            </div>);
	}
});