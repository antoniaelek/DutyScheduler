var Icon = React.createClass({

	propTypes: {
		icon: React.PropTypes.string.isRequired,
		onClick: React.PropTypes.func
	},

	onClick: function () {
		if (this.props.onClick !== undefined)
			this.props.onClick();
	},

	render: function () {
		var className = "fa fa-fw fa-" + this.props.icon;
		if (this.props.className !== undefined)
			className += " " + this.props.className;
		return <i onClick={this.onClick} className={className}></i>;
	}
});