define(["backbone",
        "sinon",
        'app/views/endpointGraph',
        'backgrid',
        'jquery',
        'underscore',
        'vis',
        'backgrid-paginator'
    ],
    function (Backbone, sinon, EndpointGraphView, Backgrid, $, _, vis) {

    describe("Endpoint Graph View", function () {

        var view, serviceCollection, serviceMessagesCollection, spy, htmlArgs, graphArgs, graphStub, graphSpy;

        describe("Render", function() {

            beforeEach(function () {
                
                serviceCollection = new Backbone.PageableCollection([{
                    InstanceLocation: ["Location1", "Location2"],
                    Name: "TestService1",
                    In: ["Message1"],
                    Out: []
                }, {
                    InstanceLocation: ["Location3"],
                    Name: "TestService2",
                    In: ["Message2"],
                    Out: ["Message1"]
                }, {
                    InstanceLocation: ["Location4"],
                    Name: "TestService3",
                    In: [""],
                    Out: ["Message2"]
                }]);
                
                serviceMessagesCollection = new Backbone.PageableCollection([{
                    Type: "Message1",
                    In: "TestService1",
                    Out: "TestService2"
                }, {
                    Type: "Message2",
                    In: "TestService2",
                    Out: "TestService3"
                }]);
                
                view = new EndpointGraphView({
                    serviceCollection: serviceCollection,
                    serviceMessagesCollection: serviceMessagesCollection
                });
                
                htmlArgs = [];
                spy = sinon.stub($.prototype, "html", function (a) {
                    htmlArgs.push(a);
                    return sinon.stub();
                });

                graphStub = sinon.stub();
                graphSpy = sinon.stub(vis, "Network", function (container, data, options) {
                    graphArgs = data;
                    return graphStub;
                });
                
                view.render();
            });

            it("should add html template to page", function () {
                expect($.prototype.html.called).toBeTruthy();
            });

            it("should build a graph model", function() {
                var nodes = view.nodes;
                var match = _.findWhere(nodes, {                        
                    id: "TestService1" 
                });
                expect(match).toBeTruthy();
                match = _.findWhere(nodes, {
                    id: "TestService2"
                });
                expect(match).toBeTruthy();
                match = _.findWhere(nodes, {
                    id: "TestService3"
                });
                expect(match).toBeTruthy();
                var edges = view.edges;
                match = _.findWhere(edges, {
                    from: "TestService2",
                    to: "TestService1",
                    label: "Message1"
                });
                expect(match).toBeTruthy();
                match = _.findWhere(edges, {
                    from: "TestService3",
                    to: "TestService2",
                    label: "Message2"
                });
                expect(match).toBeTruthy();
            });
                
            it("should create a graph using vis", function() {
                expect(vis.Network.calledOnce).toBeTruthy();
                expect(view.network).toBeTruthy();
                var nodes = graphArgs.nodes;
                var edges = graphArgs.edges;
                expect(view.nodes).toEqual(nodes);
                expect(view.edges).toEqual(edges);
                    
            });

            afterEach(function() {
                $.prototype.html.restore();
                vis.Network.restore();
            });
        });
            
    });
});