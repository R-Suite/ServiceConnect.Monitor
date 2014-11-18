define(["backbone",
        "sinon",
        'app/views/services',
        'backgrid',
        'jquery',
        'underscore',
        'backgrid-paginator'],
    function (Backbone, sinon, ServicesView, Backgrid, $, _) {

    describe("Services View", function () {

        var view, collection, spy, gridEl, gridArgs, htmlArgs, gridSpy, paginatorEl, paginatorSpy, paginatorArgs;

        describe("Render", function() {

            beforeEach(function () {
                collection = new Backbone.PageableCollection([{
                    InstanceLocation: ["Location1", "Location2"],
                    Name: "TestService",
                    In: ["In1", "In2"],
                    Out: ["Out1", "Out2"]
                }]);
                view = new ServicesView({
                    collection: collection
                });
                htmlArgs = [];
                spy = sinon.stub($.prototype, "html", function (a) {
                    htmlArgs.push(a);
                    return sinon.stub();
                });
                
                gridEl = $("<div>Test</div>");
                gridSpy = sinon.stub(Backgrid, "Grid", function (a) {
                    gridArgs = a;
                    return {
                        render: function() {
                            return {
                                $el: gridEl
                            };
                        }
                    };
                });

                paginatorEl = $("<div>Test</div>");
                paginatorSpy = sinon.stub(Backgrid.Extension, "Paginator", function (a) {
                    paginatorArgs = a;
                    return {
                        render: function () {
                            return {
                                $el: paginatorEl
                            };
                        }
                    };
                });

                view.render();
            });

            it("should add html template to page", function () {
                expect($.prototype.html.called).toBeTruthy();
            });

            describe("Grid", function() {
                it("should create service grid", function () {
                    expect(Backgrid.Grid.calledOnce).toBeTruthy();
                    var model = gridArgs.collection.first();
                    expect(model.get("Name")).toEqual("TestService");
                    expect(model.get("InstanceLocation")[0]).toEqual("Location1");
                    expect(model.get("InstanceLocation")[1]).toEqual("Location2");
                    expect(model.get("In")[0]).toEqual("In1");
                    expect(model.get("In")[1]).toEqual("In2");
                    expect(model.get("Out")[0]).toEqual("Out1");
                    expect(model.get("Out")[1]).toEqual("Out2");
                    expect(gridArgs.columns).toEqual(view.columns);
                });

                it("should add grid to page", function () {
                    expect($.prototype.html.called).toBeTruthy();
                    var el = _.find(htmlArgs, function (arg) {
                        return arg === gridEl;
                    });
                    expect(el).toBeTruthy();
                });
            });

            describe("Paginator", function() {
                it("should create service grid paginator", function () {
                    expect(Backgrid.Extension.Paginator.calledOnce).toBeTruthy();
                    var model = paginatorArgs.collection.first();
                    expect(model.get("Name")).toEqual("TestService");
                    expect(model.get("InstanceLocation")[0]).toEqual("Location1");
                    expect(model.get("InstanceLocation")[1]).toEqual("Location2");
                    expect(model.get("In")[0]).toEqual("In1");
                    expect(model.get("In")[1]).toEqual("In2");
                    expect(model.get("Out")[0]).toEqual("Out1");
                    expect(model.get("Out")[1]).toEqual("Out2");
                });

                it("should add paginator to page", function () {
                    expect($.prototype.html.called).toBeTruthy();
                    var el = _.find(htmlArgs, function (arg) {
                        return arg === paginatorEl;
                    });
                    expect(el).toBeTruthy();
                });
            });

            afterEach(function() {
                Backgrid.Grid.restore();
                Backgrid.Extension.Paginator.restore();
                $.prototype.html.restore();
            });
        });
            
    });
});