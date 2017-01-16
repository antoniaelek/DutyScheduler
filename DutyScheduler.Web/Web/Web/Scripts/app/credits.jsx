var Credits = React.createClass({

	render: function () {
		return (
			<div className="credits">
				<div>This application is a product of Distributed systems course project assignment.</div>
				<div>Following students participated in implementation of this project:</div>
				<ul className="creditsList">
					<li>Elek Antonia</li>
					<li>Divald Ljudevit</li>
					<li>Petak Magdalena</li>
					<li>Plantić Katarina</li>
				</ul>
				<Icon icon="copyright"/> Copyright 2017 Magdalena Petak
			</div>
		);
	}

});