import React, { Fragment } from "react";
import { useQuery } from "@apollo/react-hooks";
import Card from "../Card";
import gql from "graphql-tag";
import { Grid } from "@material-ui/core";

export default ({ categoryId }: { categoryId: string }) => {
  const query = categoryId
    ? useQuery(FIND_PROJECTS, {
        variables: { categoryId },
      })
    : useQuery(GET_PROJECTS);

  const { data, loading, error } = query;

  if (loading) {
    return <div>Loading projects...</div>;
  }

  if (error) {
    console.error(error);
    return;
  }

  return (
    <Fragment>
      <Grid container spacing={3}>
        {data.projects && data.projects.length ? (
          data.projects.map((project, index) => (
            <Grid item xs={12} sm={6} md={4} key={index}>
              <Card project={project} />
            </Grid>
          ))
        ) : (
          <div>No projects here yet.</div>
        )}
      </Grid>
    </Fragment>
  );
};

const GET_PROJECTS = gql`
  query {
    projects {
      id
      description
      name
      url
      category {
        colorHex
        name
      }
      descriptiveImage {
        filepath
        url
      }
      goal
      end
      remainingTime
      target
      participantCounts {
        count
      }
      status
    }
  }
`;

const FIND_PROJECTS = gql`
  query FindProjects($categoryId: [String]) {
    projects(
      where: { path: "categoryId", comparison: equal, value: $categoryId }
    ) {
      id
      name
      description
      url
      category {
        colorHex
        name
      }
      descriptiveImage {
        filepath
        url
      }
      goal
      end
      target
      remainingTime
      participantCounts {
        count
      }
      status
    }
  }
`;