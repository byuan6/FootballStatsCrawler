
function selectWhere(tbl, col, castfn, where) {
    var count = 0;
    var r = tbl.rows;
    var len = r.length;
    var values = new Array();
    for (var i = 0; i < len; i++) {
        var tr = r[i];
        if (where==null || where(tr)) {
            var text = tr.cells[col].innerText;
            var value = castfn == null ? text : castfn(text);
            if (castfn == null || !isNaN(value)) {
                values[count] = value;
                count++;
            }
        }
    }
    return values;
}
function selectRowWhere(tbl, castfn, where) {
    var count = 0;
    var r = tbl.rows;
    var len = r.length;
    var values = new Array();
    for (var i = 0; i < len; i++) {
        var tr = r[i];
        if (where==null || where(tr)) {
            var value = castfn(tr);
            values.push(value);
        }
    }
    return values;
}
function selecttake(tbl, col, max) {
    var count = 0;
    var r = tbl.rows;
    var len = r.length;
    var values = new Array();
    for (var i = 0; i < len && i < max; i++) {
        var tr = r[i];
        var value = parseFloat(tr.cells[col].innerText);
        if (!isNaN(value)) {
            values[count] = value;
            count++;
        }
    }
    return values;
}

function Isnull(replacement,castfn) {
    this.replacement = replacement;
    this.castfn = castfn;
    this.convert = function(text) {
        if(this.castfn==null)
            return text;
        else {
            var value = this.castfn(text);
            if(isNaN(value))
                return this.replacement;
            else
                return value;
        }
    };
}



function average(list) {
    var sum = 0;
    var count = 0;
    var len = list.length;
    for (var i = 0; i < len; i++) {
        var value = list[i];
        sum += value;
    }
    return sum / len;
}
function variance(list, avg) {
    var e = 0;
    if (avg == null)
        e = average(list);
    else
        e = avg;
    var sum = 0;
    var count = 0;
    var len = list.length;
    for (var i = 0; i < len; i++) {
        var value = list[i];
        var diff = value - e;
        sum += diff * diff;
    }
    return sum / len;
}
function average(list) {
    var sum = 0;
    var count = 0;
    var len = list.length;
    for (var i = 0; i < len; i++) {
        var value = list[i];
        sum += value;
    }
    return sum / len;
}
function minimum(list) {
    return Math.min(...list);
}
function maximum(list) {
    return Math.max(...list);
}
/*
function join(list) {
    var len = list.length;
    var concat = "";
    for(var i=0; i<len; i++) 
        concat+=list[i];
    return concat;
}
function Join(separator) {
    this.separator = separator;
    this.join = function(list) {
        
    };
}
function crosstab(list, xSelectorFn, ySelectorFn, zSelectorFn, aggregateFn, xlist, ylist) {
    var len = list.length;
    var w = xlist.length;
    var h = ylist.length;
    var accumul = [...Array(h).keys()].map(s=>Array(w));
    for(var i=0; i<len; i++) {
        var item = list[i];
        var col = xSelector(item);
        var row = ySelector(item);
        var x = xlist.indexOf(col);
        var y = ylist.indexOf(row);
        if(x>=0 && y>=0) {
            var value = zSelector(item);
            accumul[y][x].push(value);
        }
    }
    var result = new Array(h);
    for(var y=0; y<h; y++) {
        var record = (result[y] = new Array(w));
        for(var x=0; x<h; x++) {
            record[x] = aggregateFn(accumul[y][x]);
        }
    }
    return result;
}
*/


/* Chart js converters */
window.userChartColors = {
    midnightblue: 'rgb(25,25,112)',
	navy: 'rgb(0, 31, 63)',

	aqua: 'rgb(127, 219, 255)', //lighter than blue
	teal: 'rgb(57, 204, 204)',
	olive: 'rgb(61, 153, 112)', //darker brownish green

	fushsia: 'rgb(240, 18, 190)',
	maroon: 'rgb(133, 20, 75)',
	lime: 'rgb(1, 255, 112)',

    brown: 'rgb(139,69,19)',
    peru: 'rgb(205,133,63)',

    slategray: 'rgb(112,128,144)',
    lightslategray: 'rgb(119,136,153)',
    lightsteelblue: 'rgb(176,196,222)',

    pink: 'rgb(255,192,203)'
};


function selectToChartData(labels, seriesname, ylist, seriescolor, ...other) {
    //seriescolor = window.chartColors.red
    //var MONTHS = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    var color = Chart.helpers.color;
    var chartData = {
        //labels: ['January', 'February', 'March', 'April', 'May', 'June', 'July'],
        labels: labels,
        datasets: [{
            label: seriesname,
            backgroundColor: color(seriescolor).alpha(0.5).rgbString(),
            borderColor: seriescolor,
            borderWidth: 1,
            data: ylist
        }]
    };
    var len = other.length;
    for(var i=0; i<len; i+=3)
    {
        var nextname = other[i];
        var nextylist = other[i+1];
        var nextcolor = other[i+2];
        chartData.datasets[(i/3)+1] = {
            label: nextname,
            backgroundColor: color(nextcolor).alpha(0.5).rgbString(),
            borderColor: nextcolor,
            borderWidth: 1,
            data: nextylist
        };
    }

    return chartData;
}




function selectToScatterData(seriesname, xylist, seriescolor,isconnected, ...other) {

    //seriescolor = window.chartColors.red
    var color = Chart.helpers.color;
    var scatterChartData = {
		datasets: [{
		    label: seriesname,
		    borderColor: window.chartColors.red,
		    backgroundColor: color(window.chartColors.red).alpha(0.2).rgbString(),
		    data: xylist,
            showLine: isconnected
		}]
	};

    var len = other.length;
    for(var i=0; i<len; i+=4)
    {
        var nextname = other[i];
        var nextxylist = other[i+1];
        var nextcolor = other[i+2];
        var nextshowline = other[i+3];
        scatterChartData.datasets[(i/4)+1] = {
		    label: nextname,
		    borderColor: nextcolor,
		    backgroundColor: color(nextcolor).alpha(0.2).rgbString(),
		    data: nextxylist,
            showLine: nextshowline
        };
    }

    return scatterChartData;
}

function selectIntoWhere(tbl, where, prototype, ...col) {
    var count = 0;
    var r = tbl.rows;
    var len = r.length;
    var numcols = col.length;
    var tuples = new Array();
    for (var i = 0; i < len; i++) {
        var tr = r[i];
        if (where==null || where(tr)) {
            var values = new Array(numcols);
            for(var j=0; j<numcols; j++) {
                var text = tr.cells[col[j]].innerText;
                values[j] = text;
            }
            try {
                var tuple = new prototype(...values);
                tuples[count] = tuple;
                count++; 
            } catch(E) {
                // do nothing
            }
        }
    }
    return tuples;
}

function xy(x,y) {
    this.x=parseFloat(x);
    this.y=parseFloat(y);
    this.r=1;
    if(isNaN(this.x))
        throw new Error('x not number');
    if(isNaN(this.y))
        throw new Error('y not number');
}
function xyz(x,y,z){
    this.x=parseFloat(x);
    this.y=parseFloat(y);
    this.r=1;
    this.z = z;
    if(isNaN(this.x))
        throw new Error('x not number');
    if(isNaN(this.y))
        throw new Error('y not number');
}
function xyr(x,y,r){
    this.x=parseFloat(x);
    this.y=parseFloat(y);
    this.r = parseFloat(r);
    if(isNaN(this.x))
        throw new Error('x not number');
    if(isNaN(this.y))
        throw new Error('y not number');
    if(isNaN(this.r))
        throw new Error('r not number');
}
function xyrz(x,y,r,z){
    this.x=parseFloat(x);
    this.y=parseFloat(y);
    this.r = parseFloat(r);
    this.z = z;
    if(isNaN(this.x))
        throw new Error('x not number');
    if(isNaN(this.y))
        throw new Error('y not number');
    if(isNaN(this.r))
        throw new Error('r not number');
}


