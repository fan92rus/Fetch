import Vapi from "vuex-rest-api";

const tables = new Vapi({
  baseURL: "http://localhost/",
  state: {
    tables: [],
  },
})
  .get({
    action: "getTables",
    property: "tables",
    path: "/tables",
  })
  .post({
    property: "tables",
    action: "addPage",
    path: ({ link }) => `/tables/add/?link=${link}`,
  })
  .getStore();

export default tables;
